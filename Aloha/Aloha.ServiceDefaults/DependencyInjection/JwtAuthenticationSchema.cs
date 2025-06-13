using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Aloha.ServiceDefaults.DependencyInjection
{
    public static class JwtAuthenticationSchema
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Authentication");
            var authority = jwtSettings["Authority"];
            var audience = jwtSettings["Audience"];

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false, // Don't validate audience 
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };

                    // Add save token option to preserve the token
                    options.SaveToken = true;
                });

            return services;
        }
    }
}

