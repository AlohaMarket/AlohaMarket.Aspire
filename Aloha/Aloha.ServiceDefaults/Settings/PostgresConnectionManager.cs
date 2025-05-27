using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Aloha.ServiceDefaults.Settings
{
    public static class PostgresConnectionManager
    {
        public static void AddPostgresDbContext<TContext>(
            this IServiceCollection services, 
            IConfiguration configuration, 
            string serviceName,
            string connectionName = "DefaultConnection") 
            where TContext : DbContext
        {
            // Ensure ConnectionManager is initialized
            ConnectionManager.Initialize(configuration);
            
            // Get connection string for this service
            var connectionString = ConnectionManager.GetConnectionString(serviceName);
            
            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString, 
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));
        }
    }
}
