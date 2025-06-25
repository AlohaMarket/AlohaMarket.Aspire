using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Aloha.MicroService.Post.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        private static ILoggerFactory? _loggerFactory;

        private static class AppConsts
        {
            public static string GetConnectionStringEnvName(string service) => $"Aloha_{service}_ConnectionString";
            public static string GetPasswordEnvName(string service) => $"Aloha_{service}_Password";
            public static string GetDatabaseNameEnvName(string service) => $"Aloha_{service}_Name";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            _loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

            builder.AddServiceDefaults();
            //.AddAlohaPostgreSQL<PostDbContext>();

            builder.Services.AddSharedServicesLocal<PostDbContext>(builder.Configuration);

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
                                || e is CategoryPathInvalidEventModel;
                });
            }

            builder.Logging.AddFilter("Confluent.Kafka", LogLevel.Debug);

            return builder;
        }
    }
}
