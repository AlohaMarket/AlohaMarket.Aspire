using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace Aloha.ServiceDefaults.DependencyInjection
{
    public static class KeycloakJwtExtensions
    {
        public static IServiceCollection AddKeycloakJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
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
                    // Base JWT settings
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    // Keycloak-specific settings
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        NameClaimType = "preferred_username",
                        RoleClaimType = "roles",
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context => ExtractKeycloakRoles(context)
                    };
                });

            return services;
        }


        public static Task ExtractKeycloakRoles(TokenValidatedContext context)
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                // Extract roles from Keycloak token structure
                var roleClaims = new List<Claim>();

                // Get realm roles
                ExtractRealmRoles(context, identity, roleClaims);

                // Get client/resource roles (optional, uncomment if needed)
                // ExtractClientRoles(context, identity, roleClaims);
            }

            return Task.CompletedTask;
        }

        private static void ExtractRealmRoles(TokenValidatedContext context, ClaimsIdentity identity, List<Claim> roleClaims)
        {
            var realmAccess = context.Principal?.FindFirst("realm_access");
            if (realmAccess != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(realmAccess.Value);

                    if (doc.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                var claim = new Claim("roles", roleName);
                                roleClaims.Add(claim);
                                identity.AddClaim(claim);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if you have a logger configured
                    System.Console.WriteLine($"Error parsing Keycloak realm_access claim: {ex.Message}");
                }
            }
            
            // Add this to extract sub claim as userId
            var sub = context.Principal?.FindFirst("sub");
            if (sub != null && !string.IsNullOrEmpty(sub.Value))
            {
                identity.AddClaim(new Claim("userId", sub.Value));
            }
            
            // Add preferred_username
            var username = context.Principal?.FindFirst("preferred_username");
            if (username != null && !string.IsNullOrEmpty(username.Value))
            {
                identity.AddClaim(new Claim("userName", username.Value));
            }
        }

        private static void ExtractClientRoles(TokenValidatedContext context, ClaimsIdentity identity, List<Claim> roleClaims)
        {
            var resourceAccess = context.Principal?.FindFirst("resource_access");
            if (resourceAccess != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccess.Value);

                    // Iterate through all client role sections
                    foreach (var clientProperty in doc.RootElement.EnumerateObject())
                    {
                        string clientName = clientProperty.Name;

                        if (clientProperty.Value.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (!string.IsNullOrEmpty(roleName))
                                {
                                    // Add with client prefix for clarity
                                    var claim = new Claim("roles", $"{clientName}:{roleName}");
                                    roleClaims.Add(claim);
                                    identity.AddClaim(claim);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if you have a logger configured
                    System.Console.WriteLine($"Error parsing Keycloak resource_access claim: {ex.Message}");
                }
            }
        }
    }
}