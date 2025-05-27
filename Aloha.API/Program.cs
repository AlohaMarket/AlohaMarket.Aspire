using Aloha.ServiceDefaults.Hosting;
using Aloha.ServiceDefaults.Settings;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Initialize connection manager for environment-based connection strings
ConnectionManager.Initialize(builder.Configuration);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Example: Add MongoDB with secure connection string management
// builder.Services.AddMongoDB(builder.Configuration, "LocationService");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
