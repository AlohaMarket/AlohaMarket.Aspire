using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using MediatR;

namespace Aloha.LocationService.EventHandlers
{
    public class LocationIntegrationEventHandler(
        ILogger<LocationIntegrationEventHandler> logger,
        IEventPublisher eventPublisher) :
        IRequestHandler<TestSendEventModel>
    {
        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To=LocationService",
                request.Message, request.FromService);

            eventPublisher.PublishAsync(new TestReceiveEventModel
            {
                Message = "string 4",
                FromService = "LocationService",
                ToService = "PostService"
            });
            return Task.CompletedTask;
        }
    }
}
