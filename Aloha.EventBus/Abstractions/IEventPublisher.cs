using Aloha.EventBus.Events;

namespace Aloha.EventBus.Abstractions
{
    public interface IEventPublisher
    {
        Task<bool> PublishAsync<TEvent>(TEvent @event)
            where TEvent : IntegrationEvent;
    }
}
