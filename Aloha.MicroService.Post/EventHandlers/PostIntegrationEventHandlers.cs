using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.MicroService.Post.Infrastructure.Entity;

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
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity before update PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update location validation
                    post.IsLocationValid = true;
                    post.LocationValidationReceived = true;
                    post.LocationValidationMessage = null;
                    post.ProvinceText = request.ProvinceText;
                    post.DistrictText = request.DistrictText;
                    post.WardText = request.WardText;

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
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
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity for validation PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update location validation
                    post.IsLocationValid = false;
                    post.LocationValidationReceived = true;
                    post.LocationValidationMessage = request.ErrorMessage;

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated location validation status for PostId={PostId}", request.PostId);
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
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity for validation PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update category validation
                    post.IsCategoryValid = true;
                    post.CategoryValidationReceived = true;
                    post.CategoryValidationMessage = null;

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
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
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity for validation PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update category validation
                    post.IsCategoryValid = false;
                    post.CategoryValidationReceived = true;
                    post.CategoryValidationMessage = request.ErrorMessage;

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated category validation status for PostId={PostId}", request.PostId);
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
                var post = await dbContext.Posts
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity for validation PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update user plan validation
                    post.IsUserPlanValid = true;
                    post.UserPlanValidationReceived = true;
                    post.UserPlanValidationMessage = null;
                    post.UserPlanWasConsumed = true; // Mark that the plan was consumed

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
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
                    .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

                if (post != null)
                {
                    logger.LogInformation("Post entity for validation PostId={PostId} Status={Status} IsLocationValid={IsLocationValid} IsCategoryValid={IsCategoryValid} IsUserPlanValid={IsUserPlanValid}",
                        post.Id, post.Status, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

                    // Update user plan validation
                    post.IsUserPlanValid = false;
                    post.UserPlanValidationReceived = true;
                    post.UserPlanValidationMessage = request.ErrorMessage;

                    // Check final status and handle rollback
                    await CheckFinalStatusAndHandleRollback(post, cancellationToken);

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated user plan validation status for PostId={PostId}", request.PostId);
                }
                else
                {
                    logger.LogWarning("Post with ID {PostId} not found for user plan validation", request.PostId);
                }
            }, cancellationToken);
        }

        private async Task CheckFinalStatusAndHandleRollback(Infrastructure.Entity.Post post, CancellationToken cancellationToken)
        {
            // Only proceed if all validations have been received
            if (!post.AllValidationsReceived)
            {
                logger.LogInformation("Not all validations received yet for PostId={PostId}. LocationReceived={LocationReceived}, CategoryReceived={CategoryReceived}, UserPlanReceived={UserPlanReceived}",
                    post.Id, post.LocationValidationReceived, post.CategoryValidationReceived, post.UserPlanValidationReceived);
                return;
            }

            logger.LogInformation("All validations received for PostId={PostId}. Determining final status. IsLocationValid={IsLocationValid}, IsCategoryValid={IsCategoryValid}, IsUserPlanValid={IsUserPlanValid}",
                post.Id, post.IsLocationValid, post.IsCategoryValid, post.IsUserPlanValid);

            // Determine final status
            PostStatus finalStatus;
            if (post.IsLocationValid && post.IsCategoryValid && post.IsUserPlanValid)
            {
                finalStatus = PostStatus.Validated;
                logger.LogInformation("Post PostId={PostId} is fully validated", post.Id);
            }
            else
            {
                finalStatus = PostStatus.Invalid;
                logger.LogInformation("Post PostId={PostId} has validation failures", post.Id);

                // Handle rollback logic based on your matrix
                await HandleRollbackIfNeeded(post, cancellationToken);
            }

            // Update the final status
            post.Status = finalStatus;
        }

        private async Task HandleRollbackIfNeeded(Infrastructure.Entity.Post post, CancellationToken cancellationToken)
        {
            // Rollback conditions based on your matrix:
            // Rollback if UserPlan was valid/consumed BUT other validations failed
            var shouldRollback = post.UserPlanWasConsumed && 
                                post.IsUserPlanValid && 
                                (!post.IsLocationValid || !post.IsCategoryValid);

            if (shouldRollback)
            {
                logger.LogInformation("Publishing rollback for PostId={PostId} because UserPlan was consumed but other validations failed", post.Id);
                
                await eventPublisher.PublishAsync(new RollbackUserPlanEventModel
                {
                    PostId = post.Id,
                    UserPlanId = post.UserPlanId
                });

                logger.LogInformation("Published rollback for PostId={PostId}", post.Id);
            }
            else
            {
                logger.LogInformation("No rollback needed for PostId={PostId}. UserPlanWasConsumed={UserPlanWasConsumed}, IsUserPlanValid={IsUserPlanValid}, IsLocationValid={IsLocationValid}, IsCategoryValid={IsCategoryValid}",
                    post.Id, post.UserPlanWasConsumed, post.IsUserPlanValid, post.IsLocationValid, post.IsCategoryValid);
            }
        }
    }
}
