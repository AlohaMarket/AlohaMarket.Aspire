using Aloha.MicroService.Post.Apis;
using Aloha.MicroService.Post.Bootstraping;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.ServiceDefaults.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Post Service API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add CORS middleware - place it before other middleware
app.UseCors("AllowAll");

app.UseMiddleware<ApiExceptionHandlerMiddleware>();

// Make sure Authentication is before Authorization
app.UseAuthentication();

app.UseAuthorization();
app.MapPostApi();

app.Run();
