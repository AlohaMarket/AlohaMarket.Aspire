using Microsoft.Extensions.Configuration;
using static Aloha.AppHost.Extensions.ResourceExtensions;
using Aloha.Shared;

namespace Aloha.AppHost.Extensions;

public static class ApplicationServiceExtensions
{
    #region External Service Registration
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka");
        var mongoDb = builder.AddMongoDB("mongodb");
        var postgres = builder.AddPostgres("postgresql");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent)
                         .WithDataVolume()
                         .WithKafkaUI();
                         
            mongoDb = mongoDb.WithLifetime(ContainerLifetime.Persistent)
                            .WithDataVolume()
                            .WithMongoExpress();
                            
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent)
                             .WithDataVolume()
                             .WithPgWeb();
        }
        #endregion

        #region Create Kafka Topic
        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });
        #endregion

        #region Project References
        // PostgreSQL services
        var userDb = postgres.AddDefaultDatabase<Projects.Aloha_MicroService_User>();
        var userService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_User>()
            .WithReference(postgres)
            .WithReference(userDb, Consts.DefaultDatabase)
            .SetupKafka<Projects.Aloha_MicroService_User>(
                kafka,
                GetTopicName<Projects.Aloha_MicroService_Post>(),
                GetTopicName<Projects.Aloha_MicroService_Location>())
            .WaitFor(userDb);

        var postDb = postgres.AddDefaultDatabase<Projects.Aloha_MicroService_Post>();
        var postService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Post>()
            .WithReference(postgres)
            .WithReference(postDb, Consts.DefaultDatabase)
            .SetupKafka<Projects.Aloha_MicroService_Post>(
                kafka,
                GetTopicName<Projects.Aloha_MicroService_User>(),
                GetTopicName<Projects.Aloha_MicroService_Location>(),
                GetTopicName<Projects.Aloha_MicroService_Category>()
            )
            .WaitFor(postDb);

        var planDb = postgres.AddDefaultDatabase<Projects.Aloha_MicroService_Plan>();
        var planService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Plan>()
            .WithReference(postgres)
            .WithReference(planDb, Consts.DefaultDatabase)
            .WaitFor(planDb);

        // MongoDB services
        var locationDb = mongoDb.AddDefaultDatabase<Projects.Aloha_MicroService_Location>();
        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Location>()
            .WithReference(mongoDb)
            .WithReference(locationDb, "DefaultConnection")
            .SetupKafka<Projects.Aloha_MicroService_Location>(
                kafka,
                GetTopicName<Projects.Aloha_MicroService_Post>())
            .WaitFor(locationDb);

        var categoryDb = mongoDb.AddDefaultDatabase<Projects.Aloha_MicroService_Category>();
        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Category>()
            .WithReference(mongoDb)
            .WithReference(categoryDb, "DefaultConnection")
            .SetupKafka<Projects.Aloha_MicroService_Category>(
                kafka)
            .WaitFor(categoryDb);


        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(userService)
            .WithReference(postService)
            .WithReference(locationService)
            .WithReference(categoryService)
            //.WithReference(paymentService)
            //.WithReference(planService)
            ;
        #endregion
        return builder;
    }

    #region CreateKafkaTopics Implementation
    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();

        TopicSpecification[] topics = [
            new() { Name = GetTopicName<Projects.Aloha_MicroService_Post>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Aloha_MicroService_User>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Aloha_MicroService_Location>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Aloha_MicroService_Category>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Aloha_MicroService_Payment>(), NumPartitions = 1, ReplicationFactor = 1 },
             new() { Name = GetTopicName<Projects.Aloha_MicroService_Plan>(), NumPartitions = 1, ReplicationFactor = 1 },

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
    #endregion

}