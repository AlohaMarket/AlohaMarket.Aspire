using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Mapper;
using Aloha.MicroService.Plan.Repositories;
using Aloha.MicroService.Plan.Service;
using Aloha.Security.Authentications;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;
using Aloha.Shared.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aloha Plan Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token:"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.AddKafkaProducer("kafka");

// Register Kafka event publisher
var kafkaPublishTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
if (!string.IsNullOrWhiteSpace(kafkaPublishTopic))
{
    builder.AddKafkaEventPublisher(kafkaPublishTopic);
}
else
{
    builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
}

var kafkaConsumeTopic = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
if (!string.IsNullOrWhiteSpace(kafkaConsumeTopic))
{
    builder.AddKafkaEventConsumer(options =>
    {
        options.ServiceName = "PlanService";
        options.KafkaGroupId = "aloha-plan-service";
        options.Topics.AddRange(kafkaConsumeTopic.Split(','));
        options.IntegrationEventFactory = IntegrationEventFactory<CreateUserPlanCommand>.Instance;
        options.AcceptEvent = e => e is PostCreatedIntegrationEvent
                        || e is RollbackUserPlanEventModel || e is CreateUserPlanCommand || e is TestSendEventModel;
    });
}

builder.Services.AddSharedServices<PlanDbContext>(builder.Configuration);
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddAutoMapper(typeof(PlanProfile));

builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

// Configure the HTTP request pipeline.

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Plan Service API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add CORS middleware - place it before other middleware
app.UseCors("AllowAll");

app.UseMiddleware<ApiExceptionHandlerMiddleware>();

// Make sure Authentication is before Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

