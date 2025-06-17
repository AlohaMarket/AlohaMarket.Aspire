using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.LocationService.Data;
using Aloha.LocationService.Repositories;
using Aloha.LocationService.Services;
using Aloha.LocationService.Settings;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;
using Aloha.Shared.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace Aloha.LocationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Aloha Location Service API",
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
                options.ServiceName = "LocationService";
                options.KafkaGroupId = "aloha-location-service";
                options.Topics.AddRange(kafkaConsumeTopic.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
                options.AcceptEvent = e => e.IsEvent<TestSendEventModel>();  // Change to TestSendEventModel
            });
        }

        builder.Services.Configure<MongoSettings>(
            builder.Configuration.GetSection("MongoSettings")
        );

        builder.Services.AddSingleton<IMongoClient>(s =>
        {
            var settings = s.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        builder.Services.AddTransient<DataSeeder>();

        builder.Services.AddSingleton<MongoContext>();

        builder.Services.AddScoped<ILocationRepository, LocationRepository>();

        builder.Services.AddScoped<ILocationService, Services.LocationService>();

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            seeder.SeedAsync();
        }

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Location Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });

        }
        app.UseCors("AllowAll");

        // Use the exception handler middleware
        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        // Optional - add status code pages if needed
        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

        // After configuring Kafka
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Kafka configured with bootstrap servers: {servers}",
            builder.Configuration.GetValue<string>("Kafka:BootstrapServers"));
        logger.LogInformation("Publishing to topic: {topic}",
            builder.Configuration.GetValue<string>("EventBus:PublishingTopics"));
        logger.LogInformation("Subscribing to topics: {topics}",
            builder.Configuration.GetValue<string>("EventBus:ConsumingTopics"));
    }
}
