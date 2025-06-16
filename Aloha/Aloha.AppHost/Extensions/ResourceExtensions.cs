namespace Aloha.AppHost.Extensions;
public static class ResourceExtensions
{
    private static class Consts
    {
        public const string Env_EventPublishingTopics = "EVENT_PUBLISHING_TOPICS";
        public const string Env_EventConsumingTopics = "EVENT_CONSUMING_TOPICS";
    }
    public static IResourceBuilder<PostgresDatabaseResource> AddDefaultDatabase<TProject>(this IResourceBuilder<PostgresServerResource> builder)
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
    /// <summary>
    /// Cau hinh Kafka cho cac microservice - them tham chieu, dependencies, dang ky cac Publisher va Consumer cho cac topic.
    /// </summary>
    /// <typeparam name="TProject">Xac dinh service su dung Extension Method nay</typeparam>
    /// <param name="serviceBuilder">Builder cua service can cau hinh</param>
    /// <param name="kafkaResource">Builder cua Kafka Server Resource</param>
    /// <param name="consumingFromServices">Danh sach cac service se tieu thu topic</param>
    /// <returns>Builder da duoc cau hinh voi cac tham so can thiet</returns>
    public static IResourceBuilder<ProjectResource> SetupKafka<TProject>(
        this IResourceBuilder<ProjectResource> serviceBuilder,
        IResourceBuilder<KafkaServerResource> kafkaResource,
        params string[] consumingFromServices)
    {
        var topicNames = consumingFromServices.Select(s => s.Replace("_", "-")).ToArray();

        return serviceBuilder
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<TProject>())
            .WithEnvironment(Consts.Env_EventConsumingTopics, string.Join(',', topicNames))
            .WithReference(kafkaResource)
            .WaitFor(kafkaResource);
    }
    #endregion
}