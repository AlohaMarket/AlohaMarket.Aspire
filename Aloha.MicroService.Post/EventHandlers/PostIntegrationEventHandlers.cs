using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.MicroService.Post.Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Post.EventHandlers
{
    public class PostIntegrationEventHandlers :
    BaseEventHandler<PostDbContext, PostIntegrationEventHandlers>,
    IRequestHandler<LocationValidEventModel>,
    IRequestHandler<LocationInvalidEventModel>,
    IRequestHandler<CategoryPathValidEventModel>,
    IRequestHandler<CategoryPathInvalidEventModel>,
    IRequestHandler<UserPlanValidEventModel>,
    IRequestHandler<UserPlanInvalidEventModel>
    {

        public PostIntegrationEventHandlers(
            PostDbContext dbContext,
            IEventPublisher eventPublisher,
            ILogger<PostIntegrationEventHandlers> logger)
            : base(dbContext, logger, eventPublisher)
        {

        }

        public async Task Handle(LocationValidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received location validation success for PostId={PostId}", request.PostId);
            await ExecuteWithTransactionAsync(async () =>
            {
                var post = await dbContext.Posts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    // check if other validations have already passed to think about updating the status
                    var nextStatus = DetermineStatus(
                        isLocationValid: true,
                        isCategoryValid: post.IsCategoryValid,
                        isUserPlanValid: post.IsUserPlanValid,
                        currentStatus: post.Status);

                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsLocationValid, true)
                            .SetProperty(p => p.LocationValidationMessage, (string?)null)
                            .SetProperty(p => p.ProvinceText, request.ProvinceText)
                            .SetProperty(p => p.DistrictText, request.DistrictText)
                            .SetProperty(p => p.WardText, request.WardText)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);


                    logger.LogInformation("Location information updated for PostId={PostId}", request.PostId);
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for location validation", request.PostId);
                }
            }, cancellationToken);
        }

        public async Task Handle(LocationInvalidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogWarning("Received location validation failure for PostId={PostId}: {ErrorMessage}", request.PostId, request.ErrorMessage);

            await ExecuteWithTransactionAsync(async () =>
            {
                var post = await dbContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    var nextStatus = DetermineStatus(
                        isLocationValid: false,
                        isCategoryValid: post.IsCategoryValid,
                        isUserPlanValid: post.IsUserPlanValid,
                        currentStatus: post.Status);
                        
                    // Use patch update approach - only update the necessary fields
                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsLocationValid, false)
                            .SetProperty(p => p.LocationValidationMessage, request.ErrorMessage)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);

                    logger.LogInformation("Updated location validation status for PostId={PostId}", request.PostId);

                    // If the UserPlanValid flag is true, initiate a rollback for the user plan
                    if (post.IsUserPlanValid)
                    {
                        await eventPublisher.PublishAsync(new RollbackUserPlanEventModel
                        {
                            PostId = request.PostId,
                            UserPlanId = post.UserPlanId
                        });

                        logger.LogInformation("Published request to rollback user plan post count for PostId={PostId}",
                            request.PostId);
                    }
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for location validation", request.PostId);
                }
            }, cancellationToken);
        }

        public async Task Handle(CategoryPathValidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received category validation success for PostId={PostId}", request.PostId);

            await ExecuteWithTransactionAsync(async () =>
            {
                var post = await dbContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    var nextStatus = DetermineStatus(
                        isLocationValid: post.IsLocationValid,
                        isCategoryValid: true,
                        isUserPlanValid: post.IsUserPlanValid,
                        currentStatus: post.Status);

                    // Use patch update approach - only update the necessary fields
                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsCategoryValid, true)
                            .SetProperty(p => p.CategoryValidationMessage, (string?)null)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);

                    logger.LogInformation("Category information updated for PostId={PostId}", request.PostId);
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for category validation", request.PostId);
                }
            }, cancellationToken);
        }

        public async Task Handle(CategoryPathInvalidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogWarning("Received category validation failure for PostId={PostId}: {ErrorMessage}", request.PostId, request.ErrorMessage);

            await ExecuteWithTransactionAsync(async () =>
            {
                var post = await dbContext.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    var nextStatus = DetermineStatus(
                        isLocationValid: post.IsLocationValid,
                        isCategoryValid: false,
                        isUserPlanValid: post.IsUserPlanValid,
                        currentStatus: post.Status);
                        
                    // Use patch update approach - only update the necessary fields
                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsCategoryValid, false)
                            .SetProperty(p => p.CategoryValidationMessage, request.ErrorMessage)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);

                    logger.LogInformation("Updated category validation status for PostId={PostId}", request.PostId);

                    // If the UserPlanValid flag is true, initiate a rollback for the user plan
                    if (post.IsUserPlanValid)
                    {
                        await eventPublisher.PublishAsync(new RollbackUserPlanEventModel
                        {
                            PostId = request.PostId,
                            UserPlanId = post.UserPlanId
                        });

                        logger.LogInformation("Published request to rollback user plan post count for PostId={PostId}",
                            request.PostId);
                    }
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for category validation", request.PostId);
                }
            }, cancellationToken);
        }

        public async Task Handle(UserPlanValidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received user plan validation success for PostId={PostId}", request.PostId);

            await ExecuteWithTransactionAsync(async () =>
            {
                // Check if the post exists
                var post = await dbContext.Posts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    var nextStatus = DetermineStatus(
                        isLocationValid: post.IsLocationValid,
                        isCategoryValid: post.IsCategoryValid,
                        isUserPlanValid: true,
                        currentStatus: post.Status);

                    // Use patch update approach - only update the necessary fields
                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsUserPlanValid, true)
                            .SetProperty(p => p.UserPlanValidationMessage, (string?)null)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);

                    logger.LogInformation("User plan information updated for PostId={PostId}", request.PostId);
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for user plan validation", request.PostId);
                }
            }, cancellationToken);
        }

        public async Task Handle(UserPlanInvalidEventModel request, CancellationToken cancellationToken)
        {
            logger.LogWarning("Received user plan validation failure for PostId={PostId}: {ErrorMessage}",
                request.PostId, request.ErrorMessage);

            await ExecuteWithTransactionAsync(async () =>
            {
                var post = await dbContext.Posts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);
                
                if (post != null)
                {
                    var nextStatus = DetermineStatus(
                        isLocationValid: post.IsLocationValid,
                        isCategoryValid: post.IsCategoryValid,
                        isUserPlanValid: false,
                        currentStatus: post.Status);
                        
                    // Use patch update approach - only update the necessary fields
                    await dbContext.Posts
                        .Where(p => p.Id == request.PostId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(p => p.IsUserPlanValid, false)
                            .SetProperty(p => p.UserPlanValidationMessage, request.ErrorMessage)
                            .SetProperty(p => p.Status, nextStatus),
                            cancellationToken);

                    logger.LogInformation("Updated user plan validation status for PostId={PostId}", request.PostId);
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for user plan validation", request.PostId);
                }
            }, cancellationToken);
        }

        private PostStatus DetermineStatus(bool isLocationValid, bool isCategoryValid, bool isUserPlanValid, PostStatus currentStatus)
        {
            // If all validations pass, set to Validated
            if (isLocationValid && isCategoryValid && isUserPlanValid)
                return PostStatus.Validated;
                
            // If any validation explicitly fails, set to Invalid
            if ((!isLocationValid && currentStatus != PostStatus.PendingValidation) || 
                (!isCategoryValid && currentStatus != PostStatus.PendingValidation) ||
                (!isUserPlanValid && currentStatus != PostStatus.PendingValidation))
                return PostStatus.Invalid;
                
            // Otherwise, keep the current status (likely PendingValidation)
            return currentStatus;
        }
    }
}
