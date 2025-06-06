using Aloha.EventBus.Events;

namespace Aloha.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
}
