using Aloha.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Aloha_UserService>("aloha-userservice");

builder.Build().Run();
