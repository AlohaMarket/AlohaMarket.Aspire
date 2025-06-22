using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.EventHandlers
{
    public class UserPlanIntegrationEventHandler(
        ILogger<UserPlanIntegrationEventHandler> logger,
        IEventPublisher eventPublisher,
        PlanDbContext dbContext) :
        IRequestHandler<PostCreatedIntegrationEvent>,
        IRequestHandler<RollbackPostUsageEventModel>,
        IRequestHandler<TestSendEventModel>
    {
        public async Task Handle(PostCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling PostCreatedIntegrationEvent: PostId={PostId}, UserId={UserId}",
                request.PostId, request.UserId);
            try
            {
                // Get the user plan
                var userPlan = await dbContext.UserPlans
                    .Include(up => up.Plan)
                    .FirstOrDefaultAsync(up => up.Id == request.UserPlanId && up.UserId == request.UserId,
                        cancellationToken);

                if (userPlan == null)
                {
                    await PublishInvalidUserPlanResult(request.PostId, "User plan not found");
                    return;
                }

                // Check if user plan is active
                if (!userPlan.IsActive)
                {
                    await PublishInvalidUserPlanResult(request.PostId, "User plan is not active");
                    return;
                }

                // Check if user plan has not expired
                if (userPlan.EndDate < DateTime.UtcNow)
                {
                    await PublishInvalidUserPlanResult(request.PostId, "User plan has expired");
                    return;
                }

                // Check if user plan has remaining posts
                if (userPlan.RemainPosts <= 0)
                {
                    await PublishInvalidUserPlanResult(request.PostId, "User plan has no remaining posts");
                    return;
                }

                // Deduct one post from the remaining posts
                userPlan.RemainPosts -= 1;
                await dbContext.SaveChangesAsync(cancellationToken);

                // Publish success event
                await eventPublisher.PublishAsync(new UserPlanValidEventModel
                {
                    PostId = request.PostId,
                    RemainingPosts = userPlan.RemainPosts
                });

                logger.LogInformation("User plan validated successfully. Remaining posts: {RemainingPosts}",
                    userPlan.RemainPosts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating user plan: {Message}", ex.Message);
                await PublishInvalidUserPlanResult(request.PostId, $"Error: {ex.Message}");
            }
        }

        public async Task Handle(RollbackPostUsageEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Rolling back post usage: UserPlanId={UserPlanId}, PostId={PostId}",
                request.UserPlanId, request.PostId);

            try
            {
                // Get the user plan
                var userPlan = await dbContext.UserPlans.FindAsync(
                    new object[] { request.UserPlanId },
                    cancellationToken);

                if (userPlan == null)
                {
                    logger.LogWarning("User plan not found for rollback: UserPlanId={UserPlanId}",
                        request.UserPlanId);
                    return;
                }

                // Add back one post
                userPlan.RemainPosts += 1;
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Post usage rolled back successfully. New remaining posts: {RemainingPosts}",
                    userPlan.RemainPosts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rolling back post usage: {Message}", ex.Message);
            }
        }

        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling TestSendEventModel: {Message}", request.Message);
            // This is a test event, you can implement any logic you want here
            return Task.CompletedTask;
        }

        private async Task PublishInvalidUserPlanResult(Guid postId, string errorMessage)
        {
            await eventPublisher.PublishAsync(new UserPlanInvalidEventModel
            {
                PostId = postId,
                ErrorMessage = errorMessage
            });
        }
    }
}