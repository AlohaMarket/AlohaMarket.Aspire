using Aloha.MicroService.Payment.Models;

namespace Aloha.MicroService.Payment;

public class Program
{
    public static async Task Main(string[] args)
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
                Title = "Aloha Payment Service API",
                Version = "v1"
            });

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
            builder.Services.Configure<MongoSettings>(
            builder.Configuration.GetSection("MongoDB"));

            builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<PaymentService>();
            // Add security requirement for all operations
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

        });

        var app = builder.Build();
        // Gọi seeder
        using (var scope = app.Services.CreateScope())
        {
            var mongoSettings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
            var mongoClient = new MongoClient(mongoSettings.ConnectionString);
            var database = mongoClient.GetDatabase(mongoSettings.DatabaseName);
            var collection = database.GetCollection<Payments>("Payments");

            await MongoDbSeeder.Seed(collection); 
        }
        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Payment Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });

        }

        // Use the exception handler middleware
        app.UseMiddleware<ApiExceptionHandlerMiddleware>();
        // Optional - add status code pages if needed
        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
