using Aloha.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Aloha_EventBus_Kafka>("aloha-eventbus-kafka");

builder.Build().Run();
