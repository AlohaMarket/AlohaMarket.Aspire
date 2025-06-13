using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.ServiceDefaults.Hosting;

namespace Aloha.MicroService.Post.Bootstraping
{
    public static class ApplicationServiceExtensions
    {
        private static class Consts
        {
            public const string DefaultDatabase = "PostDatabase";
            public const string Env_EventPublishingTopics = "EventBus:PublishingTopics";
            public const string Env_EventConsumingTopics = "EventBus:ConsumingTopics";
            public const string Env_DbUsername = "DB_USERNAME";
            public const string Env_DbPassword = "DB_PASSWORD";
            public const string KafkaConfigSection = "Kafka";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddOpenApi();

            // Configure database with secure credentials
            ConfigureDatabaseConnection(builder);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            // Kafka event bus configuration
            builder.AddKafkaProducer(Consts.KafkaConfigSection);

            // Make sure bootstrap server is in the config
            var bootstrapServers = builder.Configuration.GetValue<string>($"{Consts.KafkaConfigSection}:BootstrapServers");
            if (string.IsNullOrEmpty(bootstrapServers))
            {
                // Fallback to using a local server if not configured
                builder.Configuration[$"{Consts.KafkaConfigSection}:BootstrapServers"] = "localhost:9092";
            }

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
                    options.IntegrationEventFactory = IntegrationEventFactory<PostCreatedIntegrationEvent>.Instance;
                    options.AcceptEvent = e => e.IsEvent<
                        PostStatusChangedIntegrationEvent,
                        PostActivationChangedIntegrationEvent,
                        PostPushedIntegrationEvent>();
                });
            }

            return builder;
        }
        private static void ConfigureDatabaseConnection(IHostApplicationBuilder builder)
        {
            var connectionStringTemplate = builder.Configuration.GetConnectionString(Consts.DefaultDatabase);
            var dbUsername = Environment.GetEnvironmentVariable(Consts.Env_DbUsername) ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable(Consts.Env_DbPassword) ?? "postgres";

            // Format connection string with secure credentials
            var connectionString = string.Format(connectionStringTemplate!, dbUsername, dbPassword);

            // Configure PostgreSQL DbContext with secure connection string
            builder.Services.AddDbContext<PostDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
}
