using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Infrastructure.Data;

namespace Aloha.MicroService.Post.EventHandlers
{
    public class PostIntegrationEventHandlers(
        PostDbContext dbContext,
        IEventPublisher eventPublisher,
        ILogger<PostIntegrationEventHandlers> logger) :
        IRequestHandler<TestReceiveEventModel>,
        IRequestHandler<LocationValidEventModel>,
        IRequestHandler<LocationInvalidEventModel>,
        IRequestHandler<CategoryPathValidModel>,
        IRequestHandler<CategoryPathInvalidModel>
    {
        public Task Handle(TestReceiveEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestReceiveEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);
            return Task.CompletedTask;
        }

        public async Task Handle(LocationValidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received location validation success for PostId={PostId}", request.PostId);

            var post = await dbContext.Posts.FindAsync(request.PostId);
            if (post != null)
            {
                post.IsLocationValid = true;
                post.LocationValidationMessage = null;
                post.ProvinceText = request.ProvinceText;
                post.DistrictText = request.DistrictText;
                post.WardText = request.WardText;

                // Check if post is fully validated
                if (post.IsFullyValidated)
                {
                    post.Status = Infrastructure.Entity.PostStatus.Validated;
                }

                await dbContext.SaveChangesAsync();
                logger.LogInformation("Location information updated for PostId={PostId}", request.PostId);
            }
            else
            {
                logger.LogWarning("Post with ID {PostId} not found for location validation", request.PostId);
            }
        }

        public async Task Handle(LocationInvalidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogWarning("Received location validation failure for PostId={PostId}: {ErrorMessage}",
                request.PostId, request.ErrorMessage);

            var post = await dbContext.Posts.FindAsync(request.PostId);
            if (post != null)
            {
                post.IsLocationValid = false;
                post.LocationValidationMessage = request.ErrorMessage;
                await dbContext.SaveChangesAsync();
            }
            else
            {
                logger.LogWarning("Post with ID {PostId} not found for location validation", request.PostId);
            }
        }

        public async Task Handle(CategoryPathValidModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received category validation success for PostId={PostId}", request.PostId);

            var post = await dbContext.Posts.FindAsync(request.PostId);
            if (post != null)
            {
                post.IsCategoryValid = true;
                post.CategoryValidationMessage = null;

                if (post.IsFullyValidated)
                {
                    post.Status = Infrastructure.Entity.PostStatus.Validated;
                }

                await dbContext.SaveChangesAsync();
            }
            else
            {
                logger.LogWarning("Post with ID {PostId} not found for category validation", request.PostId);
            }
        }

        public async Task Handle(CategoryPathInvalidModel request, CancellationToken cancellationToken)
        {
            logger.LogWarning("Received category validation failure for PostId={PostId}: {ErrorMessage}",
                request.PostId, request.ErrorMessage);

            var post = await dbContext.Posts.FindAsync(request.PostId);
            if (post != null)
            {
                post.IsCategoryValid = false;
                post.CategoryValidationMessage = request.ErrorMessage;
                await dbContext.SaveChangesAsync();
            }
            else
            {
                logger.LogWarning("Post with ID {PostId} not found for category validation", request.PostId);
            }
        }
    }
}
