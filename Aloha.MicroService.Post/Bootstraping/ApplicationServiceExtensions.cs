using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.ServiceDefaults.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Post.Bootstraping
{
    public static class ApplicationServiceExtensions
    {
        private static class Consts
        {
            public const string DefaultDatabase = "PostDatabase";
            public const string Env_EventPublishingTopics = "EventBus:PublishingTopics";
            public const string Env_EventConsumingTopics = "EventBus:ConsumingTopics";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddDbContext<PostDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString(Consts.DefaultDatabase);
                options.UseNpgsql(connectionString);
            });
            //builder.AddNpgsqlDbContext<PostDbContext>(Consts.DefaultDatabase);

            builder.Services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            // Kafka event bus configuration
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
                builder.AddKafkaEventConsumer(options => {
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
    }
}
