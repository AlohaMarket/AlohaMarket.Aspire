using Aloha.LocationService.Data;
using Aloha.LocationService.Repositories;
using Aloha.LocationService.Services;
using Aloha.LocationService.Settings;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Aloha.LocationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Aloha Location Service API",
                Version = "v1"
            });
        });

        builder.Services.Configure<MongoSettings>(
            builder.Configuration.GetSection("MongoSettings")
        );

        builder.Services.AddSingleton<IMongoClient>(s =>
        {
            var settings = s.GetRequiredService<IOptions<MongoSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        builder.Services.AddTransient<DataSeeder>();

        builder.Services.AddSingleton<MongoContext>();

        builder.Services.AddScoped<ILocationRepository, LocationRepository>();

        builder.Services.AddScoped<ILocationService, Services.LocationService>();

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            seeder.SeedAsync();
        }

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {

        }
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Location Service API V1");
            c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
        });

        app.UseSharedPolicies();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
