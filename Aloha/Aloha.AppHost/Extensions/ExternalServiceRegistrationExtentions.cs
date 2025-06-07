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

        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_LocationService>();

        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_CategoryService>();

        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(locationService)
            .WithReference(categoryService)            ;

        return builder;
    }
}