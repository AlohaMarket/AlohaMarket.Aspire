using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Service defaults
builder.AddServiceDefaults();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Configure HTTP logging to include headers
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    options.RequestHeaders.Add("Authorization");
    options.RequestHeaders.Add("Api-Gateway");
    options.MediaTypeOptions.AddText("application/json");
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});

// Core services
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();

// Auth (Keycloak) - IMPORTANT: This must be configured before the reverse proxy
builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

// Add reverse proxy with both transforms and message handler logging
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(transformContext =>
        {
            var httpContext = transformContext.HttpContext;
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            // Forward the Authorization header without any parsing or rewriting
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                // Clear any pre-existing Authorization header
                transformContext.ProxyRequest.Headers.Remove("Authorization");

                // Forward it as-is (already includes "Bearer ...")
                transformContext.ProxyRequest.Headers.Add("Authorization", authHeader.ToString());
            }

            // Add optional debug/custom header
            transformContext.ProxyRequest.Headers.Add("Api-Gateway", "true");

            // Log headers
            logger.LogInformation("Headers being forwarded to microservice (Api Gateway):");
            logger.LogInformation("Next Microservice: {Service}", transformContext.DestinationPrefix);
            logger.LogInformation("Forwarded Header:");
            foreach (var header in transformContext.ProxyRequest.Headers)
            {
                logger.LogInformation("{Key} = {Value}", header.Key, string.Join(", ", header.Value));
            }
            logger.LogInformation("End Forwarded Header:");

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// Middleware pipeline - ORDER IS IMPORTANT!
app.UseHttpLogging();
app.UseCors("AllowAll");

// HTTPS redirection
app.UseHttpsRedirection();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers and reverse proxy
app.MapControllers();
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseMiddleware<AuthenticatedRequestForwardingMiddleware>();
}).RequireAuthorization();


app.Run();

// Custom middleware to ensure authentication context flows to microservices
public class AuthenticatedRequestForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticatedRequestForwardingMiddleware> _logger;

    public AuthenticatedRequestForwardingMiddleware(RequestDelegate next, ILogger<AuthenticatedRequestForwardingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log all headers
        _logger.LogInformation("Incoming request Header: (AuthenForwardMiddleware):");
        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation("{Key} = {Value}", header.Key, header.Value);
        }
        _logger.LogInformation("End Incoming request Header (AuthenForwardMiddleware):");

        await _next(context);
    }
}
