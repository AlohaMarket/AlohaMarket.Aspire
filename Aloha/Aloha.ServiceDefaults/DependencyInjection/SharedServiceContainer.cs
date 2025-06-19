using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.NpgSql;

namespace Aloha.ServiceDefaults.DependencyInjection
{
    public static class SharedServiceContainer
    {
        private static ILoggerFactory? _loggerFactory;

        public static IServiceCollection AddSharedServices<TContext>
            (this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection"),
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            return services;
        }

        public static IHostApplicationBuilder AddAlohaPostgreSQL<TContext>(
            this IHostApplicationBuilder builder,
            string? configSection = null) where TContext : DbContext
        {
            _loggerFactory ??= builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

            var dbLogger = _loggerFactory?.CreateLogger("DatabaseConfiguration");
            
            configSection ??= typeof(TContext).Name.Replace("DbContext", "");
            
            string? connectionString = null;
            string? password = null;

            // 1. Truy xuat bien moi truong du vao DbContext duoc truyen vao
            connectionString = Environment.GetEnvironmentVariable($"Aloha_{configSection}_ConnectionString");
            password = Environment.GetEnvironmentVariable($"Aloha_{configSection}_Password");

            // 2. Kiem tra chuoi ket noi tu appsettings.json neu khong tim thay o moi truong
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString($"{configSection}Connection") ??
                                  builder.Configuration.GetConnectionString("SupabaseConnection") ??
                                  builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // 3. Neu van khong tim thay, su dung gia tri mac dinh va log loi
            if (string.IsNullOrEmpty(connectionString))
            {
                // Su dung gia tri mac dinh cho Postgres
                connectionString = "User Id=postgres;Password=postgres;Server=localhost;Port=5432;Database=postgres";

                dbLogger?.LogError(
                    "Không tìm thấy ConnectionString cho {ConfigSection}. Sử dụng connection string mặc định.",
                    configSection);
            }

            // Thay the [YOUR-PASSWORD] bang mat khau tu bien moi truong
            if (!string.IsNullOrEmpty(password) && connectionString.Contains("[YOUR-PASSWORD]"))
            {
                connectionString = connectionString.Replace("[YOUR-PASSWORD]", password);
            }

            #region logging trang thai connection
            // ghi log thong tin ket noi (da duoc an password, userId)
            dbLogger?.LogInformation("Configuring database for {ContextType} using section '{ConfigSection}'", 
                typeof(TContext).Name, configSection);
            dbLogger?.LogDebug("ConnectionString: {ConnectionString}", 
                Regex.Replace(
                    connectionString,
                    @"(Password|pwd|User Id|UserId|Uid)=([^;]*)",
                    "$1=*****")
            );
            #endregion

            #region Add DbContext va retry policy
            builder.Services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));
            #endregion

            #region DbHealthCheck
            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: "database", 
                    tags: new[] { "ready", "postgres", configSection.ToLowerInvariant() });
            #endregion

            return builder;
        }
    }
}
