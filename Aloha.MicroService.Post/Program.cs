using Aloha.MicroService.Post.Apis;
using Aloha.MicroService.Post.Bootstraping;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.ServiceDefaults.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapPostApi();

app.Run();
