using Aloha.ServiceDefaults.Hosting;
using Aloha.EventBus.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Kafka services
builder.AddKafkaProducer("kafka");

// Get the topic name from configuration or use a default
var topicName = builder.Configuration.GetValue<string>("Kafka:TopicName") ?? "events";
builder.AddKafkaEventPublisher(topicName);

// Configure event consumer if needed
builder.AddKafkaEventConsumer(options => {
    options.KafkaGroupId = builder.Configuration.GetValue<string>("Kafka:GroupId") ?? "event-handlers";
    options.Topics = builder.Configuration.GetSection("Kafka:Topics").Get<List<string>>() ?? new List<string> { topicName };
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Aloha.EventBus.Kafka is running!");

app.Run();
