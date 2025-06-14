using Microsoft.Extensions.Configuration;

namespace Aloha.AppHost.Extensions;

public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Registers and configures core application services, including Kafka and multiple microservices, to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder to configure.</param>
    /// <returns>The updated distributed application builder with all services registered.</returns>
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
        }

        var userService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_User>();

        var postService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Post>()
            .WithReference(userService);

        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Location>();

        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Category>();

        var paymentService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Payment>()
            .WithReference(userService);
            //.WithReference(planservice);

        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(userService)
            .WithReference(locationService)
            .WithReference(categoryService);

        return builder;
    }
}