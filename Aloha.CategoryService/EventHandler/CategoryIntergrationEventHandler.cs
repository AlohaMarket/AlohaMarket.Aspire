using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using MediatR;

namespace Aloha.CategoryService.EventHandler
{
    public class CategoryIntegrationEventHandler(
        ILogger<CategoryIntegrationEventHandler> logger,
        IEventPublisher eventPublisher) :
        IRequestHandler<TestSendEventModel>
    {
        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To=CategoryService",
                request.Message, request.FromService);

            eventPublisher.PublishAsync(new TestReceiveEventModel
            {
                Message = "string 3",
                FromService = "CategoryService",
                ToService = "PostService"
            });
            return Task.CompletedTask;
        }
    }
}