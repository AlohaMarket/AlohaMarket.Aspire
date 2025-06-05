using Microsoft.Extensions.Configuration;

namespace Aloha.AppHost.Extensions;

public static class ApplicationServiceExtensions
{
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgres("postgres");
        var kafka = builder.AddKafka("kafka");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent)
                .WithVolume("postgres-db", "var/lib/postgresql/data")
                .WithPgAdmin(pgBuilder =>
                {
                    pgBuilder.WithImageTag("latest");
                });

            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
        }

        // Add application services using helper methods
        var alohaDb = postgres.AddDefaultDatabase<Projects.Aloha_API>();

        var apiService = builder.AddProjectWithPostfix<Projects.Aloha_API>()
            .WithReference(postgres)
            .WithReference(alohaDb, "DefaultConnection");


        var eventBus = builder.AddProjectWithPostfix<Projects.Aloha_EventBus>();

        //var ;

        var locationService = builder.AddProjectWithPostfix<Projects.Aloha_LocationService>();

        var categoryService = builder.AddProjectWithPostfix<Projects.Aloha_CategoryService>();

        var notificationService = builder.AddProjectWithPostfix<Projects.Aloha_NotificationService>();

        var gatewayService = builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>()
            .WithReference(apiService)
            .WithReference(locationService)
            .WithReference(categoryService)
            ;

        return builder;
    }
}