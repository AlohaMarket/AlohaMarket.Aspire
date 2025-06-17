using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.Security.Authentications;
using Aloha.ServiceDefaults.Cloudinary;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;
using Aloha.Shared.Middlewares;
using Aloha.UserService.Data;
using Aloha.UserService.Repositories;
using Aloha.UserService.Services;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Aloha User Service API",
                Version = "v1"
            });

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token:"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new List<string>()
                }
            });
        });

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        // Register Kafka producer
        builder.AddKafkaProducer("kafka");

        // Register Kafka event publisher
        var kafkaPublishTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrWhiteSpace(kafkaPublishTopic))
        {
            builder.AddKafkaEventPublisher(kafkaPublishTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        var kafkaConsumeTopic = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrWhiteSpace(kafkaConsumeTopic))
        {
            builder.AddKafkaEventConsumer(options =>
            {
                options.ServiceName = "UserService";
                options.KafkaGroupId = "aloha-user-service";
                options.Topics.AddRange(kafkaConsumeTopic.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
                options.AcceptEvent = e => e.IsEvent<TestSendEventModel>();  // Change to TestSendEventModel
            });
        }

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
        builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
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

        // Add CORS middleware - place it before other middleware
        app.UseCors("AllowAll");

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        // Make sure Authentication is before Authorization
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
