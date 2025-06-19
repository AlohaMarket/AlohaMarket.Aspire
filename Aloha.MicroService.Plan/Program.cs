using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Mapper;
using Aloha.MicroService.Plan.Repositories;
using Aloha.MicroService.Plan.Service;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.AddAlohaPostgreSQL<PlanDbContext>();
//builder.Services.AddSharedServices<PlanDbContext>(builder.Configuration);
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
        options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
        options.AcceptEvent = e => e.IsEvent<TestSendEventModel>();
    });
}

builder.Logging.AddFilter("Confluent.Kafka", LogLevel.Debug);
var configuration = builder.Configuration;

//builder.Services.AddDbContext<PlanDbContext>(options =>
//    options.UseNpgsql(configuration.GetConnectionString("PlanConnection")));
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IPlanService, PlanService>();

builder.Services.AddAutoMapper(typeof(PlanProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseHttpsRedirection();

app.MapControllers();
app.Run();


