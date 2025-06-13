using Microsoft.Extensions.Configuration;

namespace Aloha.AppHost.Extensions;

public static class ApplicationServiceExtensions
{
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
        }

        var userService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_User>()
            .WithReference(kafka);

        var postService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Post>()
            .WithReference(userService)
            .WithReference(kafka);

        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Location>();

        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_MicroService_Category>();

        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(userService)
            .WithReference(locationService)
            .WithReference(postService)
            .WithReference(categoryService);

        return builder;
    }
}