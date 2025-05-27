using Aloha.ServiceDefaults.Exceptions;
using Aloha.ServiceDefaults.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aloha.ServiceDefaults.Middlewares
{
    public class ServiceExceptionHandlerMiddleware
    {
        private readonly ILogger<ServiceExceptionHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly string _serviceName;

        public ServiceExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ServiceExceptionHandlerMiddleware> logger,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _env = environment;
            _serviceName = configuration["ServiceName"] ?? "UnknownService";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var correlationId = GetCorrelationId(context);
                LogServiceError(correlationId, context, exception);
                await HandleServiceExceptionAsync(context, correlationId, exception);
            }
        }

        private string GetCorrelationId(HttpContext context)
        {
            return context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();
        }

        private void LogServiceError(string correlationId, HttpContext context, Exception exception)
        {
            var error = new
            {
                CorrelationId = correlationId,
                ServiceName = _serviceName,
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                SourceService = context.Request.Headers["X-Source-Service"].ToString(),
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message
            };

            var logLevel = exception switch
            {
                ServiceCommunicationException => LogLevel.Error,
                ServiceTimeoutException => LogLevel.Error,
                ServiceValidationException => LogLevel.Warning,
                _ => LogLevel.Error
            };

            _logger.Log(logLevel, exception,
                "Service Communication Error - CorrelationId: {CorrelationId} - Service: {ServiceName} - {@error}",
                correlationId,
                _serviceName,
                error);
        }

        private async Task HandleServiceExceptionAsync(HttpContext context, string correlationId, Exception exception)
        {
            var statusCode = exception switch
            {
                ServiceCommunicationException => StatusCodes.Status503ServiceUnavailable,
                ServiceTimeoutException => StatusCodes.Status504GatewayTimeout,
                ServiceValidationException => StatusCodes.Status400BadRequest,
                ServiceNotFoundException => StatusCodes.Status404NotFound,
                TooManyRequestsException => StatusCodes.Status429TooManyRequests,
                _ => StatusCodes.Status500InternalServerError
            };

            // Add Retry-After header for TooManyRequests
            if (exception is TooManyRequestsException tooManyRequestsEx && tooManyRequestsEx.RetryAfter.HasValue)
            {
                context.Response.Headers.RetryAfter = tooManyRequestsEx.RetryAfter.Value.ToString("R");
            }

            var serviceResponse = new ServiceResponse<object>
            {
                Success = false,
                CorrelationId = correlationId,
                ServiceName = _serviceName,
                Error = new ServiceError
                {
                    Code = exception.GetType().Name,
                    Message = _env.IsDevelopment()
                        ? exception.Message
                        : "Internal service error",
                    Source = _serviceName,
                    RetryAfter = (exception as TooManyRequestsException)?.RetryAfter
                }
            };

            context.Response.Headers.Append("X-Correlation-ID", correlationId);
            context.Response.Headers.Append("X-Service-Name", _serviceName);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(serviceResponse);
        }
    }

    // Extension method to add the middleware to the pipeline
    public static class ServiceExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseServiceExceptionHandler(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<ServiceExceptionHandlerMiddleware>();
        }
    }
}
