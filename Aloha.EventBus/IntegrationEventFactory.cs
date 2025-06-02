using Aloha.EventBus.Events;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace Aloha.EventBus;
public class IntegrationEventFactory : IIntegrationEventFactory
{   
    /// <summary>
    /// Tao doi tuong IntegrationEvent tu ten kieu va chuoi Json
    /// </summary>
    /// <param name="typeName">Ten day du cua kieu event can tao</param>
    /// <param name="value">Chuoi Json chua du lieu cua event</param>
    /// <returns>Doi tuong IntegrationEvent duoc tao, hoac null neu that bai</returns>
    /// <exception cref="ArgumentException">Nem ra khi khong tim thay kieu du lieu tuong ung</exception>
    public IntegrationEvent? CreateEvent(string typeName, string value)
    {
        var t = GetEventType(typeName) ?? throw new ArgumentException($"Type {typeName} not found");

        return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
    }

    /// <summary>
    /// Tim kiem kieu du lieu theo ten trong tat ca cac assembly da tai
    /// </summary>
    /// <param name="type">Ten day du cua kieu du lieu can tim</param>
    /// <returns>Doi tuong Type neu tim thay; nguoc lai la null</returns>
    private static Type? GetEventType(string type)
    {
        // most of the time the type will be in CQRS.Library.IntegrationEvents assembly
        var t = Type.GetType(type);

        return t ?? AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == type);
    }

    /// <summary>
    /// Instance singleton cua IntegrationEventFactory de su dung hieu qua va tranh khoi tao nhieu lan
    /// </summary>
    public static readonly IntegrationEventFactory Instance = new();
}

public class IntegrationEventFactory<TEvent> : IIntegrationEventFactory
{
    /// <summary>
    /// Assembly chua kieu du lieu TEvent duoc chi dinh
    /// </summary>
    private static readonly Assembly integrationEventAssembly = typeof(TEvent).Assembly;

    /// <summary>
    /// Tao doi tuong IntegrationEvent tu ten kieu va chuoi Json,
    /// duoc toi uu de tim kiem trong assembly cua TEvent truoc tien
    /// </summary>
    /// <param name="typeName">Ten day du cua kieu event can tao</param>
    /// <param name="value">Chuoi Json chua du lieu cua event</param>
    /// <returns>Doi tuong IntegrationEvent duoc tao, hoac null neu that bai</returns>
    /// <exception cref="ArgumentException">Nem ra khi khong tim thay kieu du lieu tuong ung</exception>
    public IntegrationEvent? CreateEvent(string typeName, string value)
    {
        var t = GetEventType(typeName) ?? throw new ArgumentException($"Type {typeName} not found");

        return JsonSerializer.Deserialize(value, t) as IntegrationEvent;
    }

    /// <summary>
    /// Tim kiem kieu du lieu theo ten, uu tien tim trong assembly cua TEvent,
    /// sau do moi tim trong cac assembly khac neu can thiet
    /// </summary>
    /// <param name="type">Ten day du cua kieu du lieu can tim</param>
    /// <returns>Doi tuong Type neu tim thay; nguoc lai la null</returns>
    private static Type? GetEventType(string type)
    {
        // most of the time the type will be in CQRS.Library.IntegrationEvents assembly
        var t = integrationEventAssembly.GetType(type) ?? Type.GetType(type);

        return t ?? AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.FullName == type);
    }

    /// <summary>
    /// Instance singleton cua IntegrationEventFactory<TEvent> de su dung hieu qua va tranh khoi tao nhieu lan
    /// </summary>
    public static readonly IntegrationEventFactory<TEvent> Instance = new();
}
