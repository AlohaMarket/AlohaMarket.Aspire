using MediatR;

namespace Aloha.EventBus.Events;

public class IntegrationEvent : IRequest
{
    public Guid EventId { get; private set; }
    public DateTime EventCreationDate { get; private set; }
    
    /// <summary>
    /// Initializes a new integration event with a unique version 7 GUID and the current UTC timestamp.
    /// </summary>
    public IntegrationEvent()
    {
        EventId = Guid.CreateVersion7();
        EventCreationDate = DateTime.UtcNow;
    }
}
