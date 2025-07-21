using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.NotificationService.Data;
using Aloha.NotificationService.Hubs;
using Aloha.NotificationService.Repositories;
using Aloha.NotificationService.Services;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Aloha.NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Configure MongoDB
        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings"));

        // Register MongoDB services
        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

        // Register HTTP clients for microservices
        // User and Product services removed as per requirements

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:3000", "http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
            });
        });
        builder.Services.AddSignalR();

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

        // Add memory cache and user profile cache
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IUserProfileCache, MemoryUserProfileCache>();
        builder.Services.AddSingleton<IPostInfoCache, MemoryPostInfoCache>(); // Add this line

        // Configure Kafka consumer to include PostInfoResponseEventModel
        var kafkaConsumeTopic = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
        if (!string.IsNullOrWhiteSpace(kafkaConsumeTopic))
        {
            builder.AddKafkaEventConsumer(options =>
            {
                options.ServiceName = "NotificationService";
                options.KafkaGroupId = "aloha-notification-service";
                options.Topics.AddRange(kafkaConsumeTopic.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<UserProfileResponseEventModel>.Instance;
                options.AcceptEvent = e => e.IsEvent<UserProfileResponseEventModel, PostInfoResponseEventModel>();
            });
        }

        builder.Services.AddScoped<IChatService, ChatService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Chat Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors();
        app.UseAuthorization();

        app.MapHub<NotificationHub>("/notificationHub");

        app.MapControllers();

        app.Run();
    }
}
