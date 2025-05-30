using Aloha.CategoryService.Data;
using Aloha.CategoryService.Repositories;
using Aloha.CategoryService.Services;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;

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
        Title = "Aloha Category Service API",
        Version = "v1"
    });
});

builder.Services.AddSharedServices<CategoryDbContext>(builder.Configuration);
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Category Service API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}
app.UseSharedPolicies();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
