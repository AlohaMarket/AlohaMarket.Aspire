using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;


namespace Aloha.AppHost.Extensions
{
    public static class ExternalServiceRegistrationExtentions
    {
        public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
        {
            //var cache = builder.AddRedis("redis");
            //var rabbitMq = builder.AddRabbitMq("rabbitmq");

            var postgres = builder.AddPostgres("postgres")
                                  .WithImageTag("latest")
                                  .WithVolume("postgres-db", "var/lib/postgresql/data") // Mount Docker volume to keep the data when stop the container
                                  .WithLifetime(ContainerLifetime.Persistent) // Keep the container running even if the Aspire stops
                                  .WithPgAdmin(pgBuilder =>{
                                      pgBuilder.WithImageTag("latest");
                                  });

            var AlohaDb = postgres.AddDatabase("aloha-db", "aloha");

            builder.AddProjectWithPostfix<Projects.Aloha_API>();

            builder.AddProjectWithPostfix<Projects.Aloha_ApiGateway>();

            return builder;
        }
    }
}
