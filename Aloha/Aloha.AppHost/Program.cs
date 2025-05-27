using Aloha.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Aloha_LocationService>("aloha-locationservice");

builder.Build().Run();
