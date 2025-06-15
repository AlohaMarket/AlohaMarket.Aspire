using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Infrastructure.Data;

namespace Aloha.MicroService.Post.EventHandlers
{
    public class PostIntegrationEventHandlers :
        IRequestHandler<PostStatusChangedIntegrationEvent>,
        IRequestHandler<PostActivationChangedIntegrationEvent>,
        IRequestHandler<PostPushedIntegrationEvent>,
        IRequestHandler<TestReceiveEventModel>
    {
        private readonly PostDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PostIntegrationEventHandlers> _logger;

        public PostIntegrationEventHandlers(
            PostDbContext dbContext,
            IEventPublisher eventPublisher,
            ILogger<PostIntegrationEventHandlers> logger)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task Handle(PostStatusChangedIntegrationEvent request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling post status changed event: {id}, from {previous} to {current}",
                request.PostId, request.PreviousStatus, request.CurrentStatus);

            var post = await _dbContext.Posts.Where(p => p.Id == request.PostId).SingleOrDefaultAsync(cancellationToken);
            if (post == null)
            {
                _logger.LogWarning("Post not found: {id}", request.PostId);
                return;
            }

            // Update post status
            post.Status = request.CurrentStatus;
            post.UpdatedAt = request.UpdatedAt;

            // Add status transition logic based on status changes
            if (request.PreviousStatus == PostStatus.Draft && request.CurrentStatus == PostStatus.Pending)
            {
                post.StatusMessage = "Post is pending for approval";
            }
            else if (request.PreviousStatus == PostStatus.Pending && request.CurrentStatus == PostStatus.Created)
            {
                post.StatusMessage = "Post has been approved and created";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(PostActivationChangedIntegrationEvent request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling post activation changed event: {id}, isActive: {isActive}",
                request.PostId, request.IsActive);

            var post = await _dbContext.Posts.Where(p => p.Id == request.PostId).SingleOrDefaultAsync(cancellationToken);
            if (post == null)
            {
                _logger.LogWarning("Post not found: {id}", request.PostId);
                return;
            }

            post.IsActive = request.IsActive;
            post.UpdatedAt = request.UpdatedAt;
            post.StatusMessage = request.IsActive ? "Post is now active" : "Post has been deactivated";

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(PostPushedIntegrationEvent request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling post pushed event: {id}, pushedAt: {pushedAt}",
                request.PostId, request.PushedAt);

            var post = await _dbContext.Posts.Where(p => p.Id == request.PostId).SingleOrDefaultAsync(cancellationToken);
            if (post == null)
            {
                _logger.LogWarning("Post not found: {id}", request.PostId);
                return;
            }

            post.PushedAt = request.PushedAt;
            post.UpdatedAt = DateTime.UtcNow;
            post.StatusMessage = "Post has been pushed to external systems";

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task Handle(TestReceiveEventModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TestReceiveEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);
            return Task.CompletedTask;
        }
    }
}
