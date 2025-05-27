using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Aloha.ServiceDefaults.Settings
{
    public static class ConnectionManager
    {
        private static readonly Dictionary<string, string> _connectionStrings = new();
        private static bool _initialized = false;

        public static void Initialize(IConfiguration configuration)
        {
            if (_initialized) return;
            
            // Load environment variables from .env files based on environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Env.Load();
            Env.Load($".env.{environment}");
            Env.TraversePath().Load(".env.local"); // This will look for the file but won't error if not found
            
            // Register connection strings from environment variables
            foreach (var service in new[] { "API", "LocationService", "CategoryService" })
            {
                var envVarName = $"SUPABASE_{service.ToUpperInvariant()}_CONNECTION";
                var connectionString = Environment.GetEnvironmentVariable(envVarName);
                
                if (!string.IsNullOrEmpty(connectionString))
                {
                    _connectionStrings[service] = connectionString;
                }
            }
            
            _initialized = true;
        }
        
        public static string GetConnectionString(string serviceName)
        {
            if (!_initialized)
                throw new InvalidOperationException("ConnectionManager not initialized. Call Initialize first.");
                
            return _connectionStrings.TryGetValue(serviceName, out var connectionString) 
                ? connectionString 
                : throw new KeyNotFoundException($"No connection string found for {serviceName}");
        }
    }
}
