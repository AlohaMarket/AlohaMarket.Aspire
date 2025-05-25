var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImageTag("latest")
    .WithVolume("postgres-db", "var/lib/postgresql/data") // Mount Docker volume to keep the data when stop the container
    .WithLifetime(ContainerLifetime.Persistent) // Keep the container running even if the Aspire stops
    .WithPgAdmin(pgBuilder => {
        pgBuilder.WithImageTag("latest");
    });

var AlohaDb = postgres.AddDatabase("aloha-db","aloha");

builder.AddProject<Projects.Aloha_API>("aloha-api");

builder.Build().Run();
