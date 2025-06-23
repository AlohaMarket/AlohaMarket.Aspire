using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Aloha.UserService.EventHandler
{
    public class UserIntegrationEventHandlers(
        ILogger<UserIntegrationEventHandlers> logger,
        IEventPublisher eventPublisher) :
        IRequestHandler<TestSendEventModel>
    {
        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);

            eventPublisher.PublishAsync(new TestReceiveEventModel
            {
                Message = "string 2",
                FromService = "UserService",
                ToService = "PostService"
            });
            return Task.CompletedTask;
        }
    }
}