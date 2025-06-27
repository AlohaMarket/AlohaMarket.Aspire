using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.PostService.Data;
using Aloha.PostService.Repositories;
using Aloha.PostService.Services;
using Aloha.Security.Authentications;
using Aloha.ServiceDefaults.Cloudinary;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.Shared;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace Aloha.PostService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddAuthorization();
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
                Title = "Aloha Post Service API",
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

        builder.AddKafkaProducer("kafka");

        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrEmpty(kafkaTopic))
        {
            builder.AddKafkaEventPublisher(kafkaTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrEmpty(eventConsumingTopics))
        {
            builder.AddKafkaEventConsumer(options =>
            {
                options.ServiceName = "PostService";
                options.KafkaGroupId = "aloha-post-service";
                options.Topics.AddRange(eventConsumingTopics.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<PostCreatedIntegrationEvent>.Instance;
                options.AcceptEvent = e =>
                    e is LocationValidEventModel
                    || e is LocationInvalidEventModel
                    || e is CategoryPathValidEventModel
                    || e is CategoryPathInvalidEventModel
                    || e is UserPlanInvalidEventModel
                    || e is UserPlanValidEventModel;
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

        // Register application services
        builder.Services.AddSharedServicesLocal<PostDbContext>(builder.Configuration);
        builder.Services.AddScoped<IPostRepository, PostRepository>();
        builder.Services.AddScoped<IPostService, Services.PostService>();
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Post Service API V1");
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
