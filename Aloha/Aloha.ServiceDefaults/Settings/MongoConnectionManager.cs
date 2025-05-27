using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Aloha.ServiceDefaults.Settings
{
    public static class MongoConnectionManager
    {
        public static void AddMongoDB(this IServiceCollection services, IConfiguration configuration, string serviceName)
        {
            // Ensure ConnectionManager is initialized
            ConnectionManager.Initialize(configuration);
            
            // Get connection string for this service
            var connectionString = ConnectionManager.GetConnectionString(serviceName);
            
            // Register MongoDB client
            services.Configure<MongoSettings>(options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseName = configuration.GetValue<string>($"MongoSettings:DatabaseName") 
                    ?? $"{serviceName}Db";
            });

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });
        }
    }

    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
