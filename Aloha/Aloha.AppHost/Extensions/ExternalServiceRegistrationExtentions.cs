using Microsoft.Extensions.Configuration;

namespace Aloha.AppHost.Extensions;

public static class ApplicationServiceExtensions
{
#region Constants
    private static class Consts
    {
        public const string Env_EventPublishingTopics = "EVENT_PUBLISHING_TOPICS";
        public const string Env_EventConsumingTopics = "EVENT_CONSUMING_TOPICS";
    }
#endregion

#region External Service Registration
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka")
            .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent)
                         .WithDataVolume()
                         .WithKafkaUI();
        }
#endregion

#region Create Kafka Topic
        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });
#endregion

#region Project References
        var userService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_User>()
            .SetupKafka<Projects.Aloha_MicroService_User>(
                kafka,
                GetTopicName<Projects.Aloha_MicroService_Post>(),
                GetTopicName<Projects.Aloha_MicroService_Location>());

        var postService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Post>()
            .SetupKafka<Projects.Aloha_MicroService_Post>(
                kafka,
                GetTopicName<Projects.Aloha_MicroService_User>(),
                GetTopicName<Projects.Aloha_MicroService_Location>()
            );

        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Location>()
            .SetupKafka<Projects.Aloha_MicroService_Location>(
                kafka);

        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Category>()
            .SetupKafka<Projects.Aloha_MicroService_Category>(
                kafka);

        var paymentService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Payment>()
            .WithReference(userService)
            //.WithReference(planservice);
            .SetupKafka<Projects.Aloha_MicroService_Payment>(
                kafka);

        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(userService)
            .WithReference(postService)
            .WithReference(locationService)
            .WithReference(categoryService)
            .WithReference(paymentService);
#endregion

        return builder;
    }

    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();

        TopicSpecification[] topics = [
            new() { Name = GetTopicName<Projects.Aloha_MicroService_Post>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Aloha_MicroService_User>(), NumPartitions = 1, ReplicationFactor = 1 },
        ];

        logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));

        var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = connectionString,
        }).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions() { });
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "An error occurred creating topics");
        }
    }

    private static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";

    /// <summary>
    /// Cau hinh Kafka cho cac microservice - them tham chieu, dependencies, dang ky cac Publisher va Consumer cho cac topic.
    /// </summary>
    /// <typeparam name="TProject">Xac dinh service su dung Extension Method nay</typeparam>
    /// <param name="serviceBuilder">Builder cua service can cau hinh</param>
    /// <param name="kafkaResource">Builder cua Kafka Server Resource</param>
    /// <param name="consumingFromServices">Danh sach cac service se tieu thu topic</param>
    /// <returns>Builder da duoc cau hinh voi cac tham so can thiet</returns>
    private static IResourceBuilder<ProjectResource> SetupKafka<TProject>(
        this IResourceBuilder<ProjectResource> serviceBuilder,
        IResourceBuilder<KafkaServerResource> kafkaResource,
        params string[] consumingFromServices)
    {
        var topicNames = consumingFromServices.Select(s => s.Replace("_", "-")).ToArray();

        return serviceBuilder
        .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<TProject>())
        .WithEnvironment(Consts.Env_EventConsumingTopics, string.Join(',', topicNames))
        .WithReference(kafkaResource)
        .WaitFor(kafkaResource);
    }
}