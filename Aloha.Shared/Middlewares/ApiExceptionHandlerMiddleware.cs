using Aloha.Shared.Exceptions;
using Aloha.Shared.Meta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aloha.Shared.Middlewares
{
    public class ApiExceptionHandlerMiddleware
    {
        // Fields
        private readonly ILogger<ApiExceptionHandlerMiddleware> _logger; // for logging
        private readonly RequestDelegate _next; // for the next middleware
        private readonly IHostEnvironment _env; // for

        //constructor
        public ApiExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<ApiExceptionHandlerMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _env = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestStartTime = DateTime.UtcNow;
            var requestId = context.TraceIdentifier;

            try
            {
                // Initial request logging
                _logger.LogInformation(
                    "ApiExceptionMiddleware: Starting request processing - RequestId: {RequestId}, Path: {Path}, Method: {Method}",
                    requestId,
                    context.Request.Path,
                    context.Request.Method);

                // Check API Gateway
                bool isFromGateway = context.Request.Headers.TryGetValue("Api-Gateway", out var gatewayHeader);
                _logger.LogInformation(
                    "Request source: {Source}",
                    isFromGateway ? "API Gateway" : "Direct Client");

                // Authentication status
                var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
                _logger.LogInformation(
                    "Authentication Status - IsAuthenticated: {IsAuthenticated}, Identity: {IdentityName}",
                    isAuthenticated,
                    context.User?.Identity?.Name ?? "none");

                await _next(context);

                // Log response status
                var requestDuration = DateTime.UtcNow - requestStartTime;
                _logger.LogInformation(
                    "Request completed - RequestId: {RequestId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                    requestId,
                    context.Response.StatusCode,
                    requestDuration.TotalMilliseconds);
            }
            catch (Exception exception)
            {
                var errorId = Guid.NewGuid().ToString();
                _logger.LogError(
                    exception,
                    "Unhandled exception - RequestId: {RequestId}, ErrorId: {ErrorId}, Message: {Message}",
                    requestId,
                    errorId,
                    exception.Message);

                LogError(errorId, context, exception);
                await HandleExceptionAsync(context, errorId, exception);
            }
        }

        private void LogError(string errorId, HttpContext context, Exception exception)
        {
            var error = new
            {
                ErrorId = errorId,
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
            };

            var logLevel = exception switch
            {
                BusinessException => LogLevel.Warning,
                ValidationException => LogLevel.Warning,
                NotFoundException => LogLevel.Information,
                _ => LogLevel.Error
            };

            _logger.Log(logLevel, exception,
                "Error ID: {ErrorId} - Path: {Path} - Method: {Method} - {@error}",
                errorId,
                context.Request.Path,
                context.Request.Method,
                error);
        }

        private async Task HandleExceptionAsync(HttpContext context, string errorId, Exception exception)
        {
            var statusCode = exception switch
            {
                ApiException apiEx => (int)apiEx.StatusCode,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            // Add specific handling for TooManyRequestsException
            if (exception is TooManyRequestsException tooManyRequestsEx && tooManyRequestsEx.RetryAfter.HasValue)
            {
                // Add Retry-After header in seconds
                var retryAfterSeconds = Math.Max(1, (int)(tooManyRequestsEx.RetryAfter.Value - DateTime.UtcNow).TotalSeconds);
                context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());

                // Optionally, you can also add the date format
                context.Response.Headers.Append("X-RateLimit-Reset", tooManyRequestsEx.RetryAfter.Value.ToString("r"));
            }

            var message = _env.IsDevelopment() || exception is ApiException
                ? exception.Message
                : "An unexpected error occurred";

            var errorResponse = ApiResponseBuilder.BuildResponse(
                data: new
                {
                    ErrorId = errorId,
                    Timestamp = DateTime.UtcNow,
                    RetryAfter = exception is TooManyRequestsException tooManyReq ? tooManyReq.RetryAfter : null
                },
                message: message
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }

    // Extension method to add the middleware to the pipeline
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiExceptionHandlerMiddleware>();
        }
    }
}
