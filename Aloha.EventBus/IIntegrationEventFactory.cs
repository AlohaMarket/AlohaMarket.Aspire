using Aloha.EventBus.Events;

namespace Aloha.EventBus;
public interface IIntegrationEventFactory
{
    IntegrationEvent? CreateEvent(string typeName, string value);
}
