using Aloha.ServiceDefaults.Cloudinary;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.ServiceDefaults.Middlewares;
using Aloha.UserService.Data;
using Aloha.UserService.Repositories;
using Aloha.UserService.Services;
using dotenv.net;
using Microsoft.OpenApi.Models;

namespace Aloha.UserService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        // Add CORS configuration
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();


        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Aloha User Service API",
                Version = "v1"
            });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{builder.Configuration["Authentication:Authority"]}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{builder.Configuration["Authentication:Authority"]}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                                { "openid", "OpenID" },
                                { "profile", "Profile" }
                        }
                    }
                }
            });

            // Add security requirement for all operations
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "openid", "profile" }
                    }
            });
        });

        DotEnv.Load(options: new DotEnvOptions(
        envFilePaths: new[] { Path.Combine(Directory.GetCurrentDirectory(), "..", ".env") }));

        // Add Cloudinary config from environment
        builder.Services.Configure<CloudinarySettings>(options =>
        {
            options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUDNAME") ?? throw new InvalidOperationException("CLOUDINARY_CLOUDNAME is not set in environment variables.");
            options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_APIKEY") ?? throw new InvalidOperationException("CLOUDINARY_APIKEY is not set in environment variables.");
            options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_APISECRET") ?? throw new InvalidOperationException("CLOUDINARY_APISECRET is not set in environment variables.");
        });

        builder.Services.AddSharedServices<UserDbContext>(builder.Configuration);
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, Services.UserService>();
        builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        // Replace the JWT auth configuration with the simplified extension
        builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha User Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        // Add detailed logger for auth debugging
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Request path: {Path}", context.Request.Path);
            logger.LogInformation("Request to User Service");
            foreach (var header in context.Request.Headers)
            {

                logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
            }

            await next();
        });

        // Add CORS middleware - place it before other middleware
        app.UseCors("AllowAll");

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        // Make sure Authentication is before Authorization
        app.UseAuthentication();
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var user = context.User;

            logger.LogInformation("IsAuthenticated: {IsAuthenticated}", user.Identity?.IsAuthenticated);
            logger.LogInformation("Authentication Type: {AuthType}", user.Identity?.AuthenticationType ?? "none");

            // JWT specific debugging
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                logger.LogInformation("Auth header present: {Length} chars", authHeader.Length);

                // Check if bearer token
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Bearer token found");
                }
            }

            // Log all claims
            logger.LogInformation("Claims count: {Count}", user.Claims?.Count() ?? 0);
            foreach (var claim in user.Claims)
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            await next();
        });
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
