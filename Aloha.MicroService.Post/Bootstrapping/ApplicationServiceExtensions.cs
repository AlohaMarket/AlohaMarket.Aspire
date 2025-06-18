using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;


namespace Aloha.MicroService.Post.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        private static class AppConsts
        {
            public const string DefaultDatabase = "PostDatabase";
            public const string Env_DbUsername = "DB_USERNAME";
            public const string Env_DbPassword = "DB_PASSWORD";
            public const string Env_DatabaseConnection = "Aloha_PostDB_ConnectionString";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {            builder.AddServiceDefaults()
                   .AddAlohaPostgreSQL<PostDbContext>();

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
            });            // Database configuration is handled in AddAlohaPostgreSQL

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            // Kafka event bus configuration
            builder.AddKafkaProducer("kafka");

            // Configure Kafka event publisher
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
                    options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
                    options.AcceptEvent = e => e.IsEvent<
                        TestReceiveEventModel>();
                });
            }

            builder.Logging.AddFilter("Confluent.Kafka", LogLevel.Debug);

            return builder;
        }        public static IHostApplicationBuilder AddAlohaPostgreSQL<TContext> (
            this IHostApplicationBuilder builder,
            string? dbConnection = null,
            string? dbUsername = null,
            string? dbPassword = null
            ) where TContext : DbContext
        {
            // lay contextion string tu tham so hoac bien moi truong
            // neu ca bien moi truong khong ton tai thi se su dung gia tri mac dinh
            var connectionStringTemplate = dbConnection 
                                        ?? Environment.GetEnvironmentVariable(AppConsts.Env_DatabaseConnection)
                                        ?? builder.Configuration.GetConnectionString(AppConsts.DefaultDatabase);
            
            // tuong tu voi connection string, lay username va password tu tham so hoac bien moi truong hoac gia tri mac dinh
            var username = dbUsername 
                        ?? Environment.GetEnvironmentVariable(AppConsts.Env_DbUsername) 
                        ?? "postgres";
            
            var password = dbPassword 
                        ?? Environment.GetEnvironmentVariable(AppConsts.Env_DbPassword) 
                        ?? "postgres";

            // ghep connection string voi username va password
            var connectionString = string.Format(connectionStringTemplate!, username, password);

            // tien hanh dang ky DbContext voi connection string da tao
            builder.Services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString));                
            builder.Services.AddScoped<TContext>();
            return builder;
        }
    }
}
