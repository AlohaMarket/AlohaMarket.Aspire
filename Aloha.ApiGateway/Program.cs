using Aloha.ApiGateway.Services;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Core Services
builder.Services.AddControllers();
builder.Services.AddSingleton<SwaggerMergeService>();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aloha API Gateway",
        Version = "v1",
        Description = "Gateway API for Aloha Market services"
    });

    // OAuth2 Security (Keycloak)
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{builder.Configuration["Authentication:Authority"]}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{builder.Configuration["Authentication:Authority"]}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile" }
        }
    });

    c.DocInclusionPredicate((_, api) => true); // Include all endpoints
});

// Reverse Proxy (YARP)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Auth
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Swagger UI with downstream docs via proxy
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    var downstreamSwagger = builder.Configuration
        .GetSection("DownstreamSwaggerUrls")
        .Get<Dictionary<string, string>>();

    foreach (var (key, _) in downstreamSwagger)
    {
        var proxyPath = $"/api/{key.ToLower()}/swagger/v1/swagger.json";
        c.SwaggerEndpoint(proxyPath, $"{key} Service");
    }

    c.RoutePrefix = string.Empty;
    c.OAuthClientId(builder.Configuration["Authentication:Audience"]);
    c.OAuthAppName("Aloha API Gateway");
    c.OAuthUsePkce();
});

app.MapGet("/swagger/v1/merged.json", async (SwaggerMergeService merger) =>
{
    var doc = await merger.GetMergedSwaggerAsync();
    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);
    var openApiWriter = new Microsoft.OpenApi.Writers.OpenApiJsonWriter(writer);

    doc.SerializeAsV3(openApiWriter);
    await writer.FlushAsync();
    stream.Position = 0;

    return Results.Stream(stream, "application/json");
});

// Middleware pipeline
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

app.Run();
