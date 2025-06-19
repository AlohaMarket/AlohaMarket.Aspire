using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Infrastructure.Data;

namespace Aloha.MicroService.Post.EventHandlers
{
    public class PostIntegrationEventHandlers(
        PostDbContext dbContext,
        IEventPublisher eventPublisher,
        ILogger<PostIntegrationEventHandlers> logger) :
        IRequestHandler<TestReceiveEventModel>
    {
        public Task Handle(TestReceiveEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestReceiveEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);
            return Task.CompletedTask;
        }
    }
}
