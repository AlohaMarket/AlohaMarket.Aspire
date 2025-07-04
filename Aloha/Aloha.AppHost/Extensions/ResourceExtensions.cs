﻿namespace Aloha.AppHost.Extensions;
public static class ResourceExtensions
{
    private static ILogger<Program>? _logger;

    public static IResourceBuilder<PostgresDatabaseResource> AddDefaultDatabase<TProject>(this IResourceBuilder<PostgresServerResource> builder)
    {
        return builder.AddDatabase($"{typeof(TProject).Name.Replace('_', '-')}-db");
    }

    public static IResourceBuilder<MongoDBDatabaseResource> AddDefaultDatabase<TProject>(this IResourceBuilder<MongoDBServerResource> builder)
    {
        return builder.AddDatabase($"{typeof(TProject).Name.Replace('_', '-')}-db");
    }

    public static IResourceBuilder<ProjectResource> AddProjectWithPostfix<TProject>(this IDistributedApplicationBuilder builder, string postfix = "")
        where TProject : IProjectMetadata, new()
    {
        if (string.IsNullOrEmpty(postfix))
        {
            return builder.AddProject<TProject>(typeof(TProject).Name.Replace('_', '-'));
        }
        else
        {
            return builder.AddProject<TProject>(typeof(TProject).Name.Replace('_', '-') + "-" + postfix);
        }
    }

    public static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";

    #region setupKafka
    
    // Cau hinh Kafka cho cac microservice - them tham chieu, dependencies, dang ky cac Publisher va Consumer cho cac topic.
    public static IResourceBuilder<ProjectResource> SetupKafka<TProject>(
        this IResourceBuilder<ProjectResource> serviceBuilder,
        IResourceBuilder<KafkaServerResource> kafkaResource,
        params string[] consumingFromServices)
    {
        // Check ten topic de dang ky Publisher 
        var publishingTopic = GetTopicName<TProject>();
        if (string.IsNullOrWhiteSpace(publishingTopic))
        {
            throw new ArgumentException($"Invalid publishing topic name for {typeof(TProject).Name}");
        }

        // xu ly danh sach cac topic ma service nay se tieu thu (loai bo cac topic trung lap va chuan hoa ten)
        var topicNames = consumingFromServices
            .Select(s => s.Replace("_", "-"))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // chuan hoa ten topic tieu thu khi chuyen thanh dinh dang json de dang ky cac topic se tieu thu
        // neu service nay khong tieu thu topic nao thi se set la empty string
        var consumingTopics = topicNames.Length > 0 
            ? string.Join(',', topicNames) 
            : string.Empty;

        return serviceBuilder
            .WithEnvironment(Consts.Env_EventPublishingTopics, publishingTopic)
            .WithEnvironment(Consts.Env_EventConsumingTopics, consumingTopics)
            .WithReference(kafkaResource)
            .WaitFor(kafkaResource);
    }
    #endregion

    #region setupDatabase
    public static IResourceBuilder<ProjectResource> SetupPostgresDb<TProject>(
        this IResourceBuilder<ProjectResource> serviceBuilder,
        IResourceBuilder<PostgresServerResource> postgres,
        IResourceBuilder<PostgresDatabaseResource> database)
    {
        if (database == null)
        {
            _logger?.LogError(
                "Database resource is null for {ServiceType}. Database configuration might fail.",
                typeof(TProject).Name);
            return serviceBuilder;
        }

        _logger?.LogInformation("Configuring PostgreSQL for {ServiceType}", typeof(TProject).Name);

        return serviceBuilder
            .WithReference(postgres)
            .WithReference(database, Consts.DefaultDatabase)
            .WaitFor(database);
    }

    public static IResourceBuilder<ProjectResource> SetupMongoDb<TProject>(
        this IResourceBuilder<ProjectResource> serviceBuilder,
        IResourceBuilder<MongoDBServerResource> mongodb,
        IResourceBuilder<MongoDBDatabaseResource> database)
    {
        if (database == null)
        {
            _logger?.LogError(
                "Database resource is null for {ServiceType}. Database configuration might fail.",
                typeof(TProject).Name);
            return serviceBuilder;
        }

        _logger?.LogInformation("Configuring MongoDB for {ServiceType}", typeof(TProject).Name);

        return serviceBuilder
            .WithReference(mongodb)
            .WithReference(database, Consts.DefaultDatabase)
            .WaitFor(database);
    }
    #endregion
}