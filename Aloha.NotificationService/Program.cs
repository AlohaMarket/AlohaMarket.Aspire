using Aloha.NotificationService.Hubs;
using Aloha.NotificationService.Services;
using Aloha.ServiceDefaults.Hosting;
using Aloha.NotificationService.Data;
using Aloha.NotificationService.Repositories;

namespace Aloha.NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Configure MongoDB
        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("MongoDbSettings"));

        // Register MongoDB services
        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

        // Register HTTP clients for microservices
        // User and Product services removed as per requirements

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:3000", "http://localhost:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
        builder.Services.AddSignalR();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddScoped<IChatService, ChatService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors();
        app.UseAuthorization();

        app.MapHub<NotificationHub>("/notificationHub");

        app.MapControllers();

        app.Run();
    }
}
