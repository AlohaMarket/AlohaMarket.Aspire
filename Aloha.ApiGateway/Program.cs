using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Service defaults
builder.AddServiceDefaults();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Core services
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();

// YARP Reverse Proxy
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Auth (Keycloak)
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

// CORS and HTTPS
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();

// Endpoint routing
app.MapControllers();
app.MapReverseProxy();
app.UseSwagger();

// Swagger UI using merged doc only
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/merged.json", "Unified Aloha API v1"); // FIXED
    c.RoutePrefix = ""; // serve at root
    c.DocumentTitle = "Aloha API Gateway";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DisplayRequestDuration();
    c.DisplayOperationId();
    c.EnableFilter();
});

app.Run();
