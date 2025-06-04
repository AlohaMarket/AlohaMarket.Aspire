using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Aloha.ServiceDefaults.Middlewares
{
    public class ListenToOnlyApiGatewayMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is coming from the API Gateway
            var signedHeader = context.Request.Headers["Api-Gateway"];
            //check if null => this is not from the API Gateway
            if (signedHeader.FirstOrDefault() is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Service Unavailable. Please try again later.");
                return;
            }
            else
            {
                // if it is from the API Gateway => continue to the next middleware
                await next(context);
            }
        }
    }

    public static class ListenToOnlyApiGatewayMiddlewareExtensions
    {
        public static IApplicationBuilder UseListenToOnlyApiGateway(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ListenToOnlyApiGatewayMiddleware>();
        }
    }
}
