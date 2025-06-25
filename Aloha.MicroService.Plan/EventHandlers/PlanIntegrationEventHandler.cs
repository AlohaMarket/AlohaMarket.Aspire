using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.EventHandlers
{
    public class PlanIntegrationEventHandler :
        BaseEventHandler<PlanDbContext, PlanIntegrationEventHandler>,
        IRequestHandler<CreateUserPlanCommand>,
        IRequestHandler<PostCreatedIntegrationEvent>,
        IRequestHandler<RollbackUserPlanEventModel>
    {
        public PlanIntegrationEventHandler(
            PlanDbContext dbContext,
            ILogger<PlanIntegrationEventHandler> logger,
            IEventPublisher eventPublisher
        ) : base(dbContext, logger, eventPublisher)
        {
        }

        public async Task Handle(CreateUserPlanCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received CreateUserPlanCommand: PaymentId={PaymentId}, UserId={UserId}, PlanId={PlanId}, Amount={Amount}",
                request.PaymentId, request.UserId, request.PlanId, request.Amount);

            Guid? userPlanId = null;
            bool isSuccess = false;
            string message = string.Empty;

            await ExecuteWithTransactionAsync(async () =>
            {
                var plan = await dbContext.Plans
                    .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

                if (plan == null)
                {
                    message = "Plan not found.";
                    return;
                }

                var now = DateTime.UtcNow;
                var userPlan = new UserPlan
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    PlanId = plan.Id,
                    StartDate = now,
                    EndDate = now.AddDays(plan.DurationDays),
                    RemainPosts = plan.MaxPosts,
                    RemainPushes = plan.MaxPushes,
                    IsActive = true,
                    CreateAt = now,
                    PaymentId = request.PaymentId
                };

                logger.LogInformation("Creating new UserPlan with Id: {Id}", userPlan.Id);
                dbContext.UserPlans.Add(userPlan);
                await dbContext.SaveChangesAsync(cancellationToken);

                userPlanId = userPlan.Id;
                isSuccess = true;
                message = "User plan provisioned successfully.";
            }, cancellationToken);

            await eventPublisher.PublishAsync(new UserPlanProvisioningResultEvent
            {
                PaymentId = request.PaymentId,
                UserId = request.UserId,
                PlanId = request.PlanId,
                IsSuccess = isSuccess,
                Message = message,
                UserPlanId = userPlanId
            });
        }

        public async Task Handle(PostCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating user plan for PostId={PostId}", request.PostId);

            bool isValid = false;
            string errorMessage = string.Empty;

            try
            {
                await ExecuteWithTransactionAsync(async () =>
                {
                    var userPlan = await dbContext.UserPlans
                        .FirstOrDefaultAsync(up => up.Id == request.UserPlanId, cancellationToken);

                    if (userPlan == null)
                    {
                        errorMessage = "User plan not found";
                        return;
                    }

                    if (!userPlan.IsActive)
                    {
                        errorMessage = "User plan is not active";
                        return;
                    }

                    if (userPlan.RemainPosts <= 0)
                    {
                        errorMessage = "No remaining posts in plan";
                        return;
                    }

                    if (userPlan.EndDate <= DateTime.UtcNow)
                    {
                        errorMessage = "User plan has expired";
                        return;
                    }

                    // Decrement the remaining posts count
                    userPlan.RemainPosts -= 1;

                    // If the plan has no remaining posts, deactivate it
                    if (userPlan.RemainPosts <= 0)
                    {
                        userPlan.IsActive = false;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("User plan updated for PostId={PostId}, UserPlanId={UserPlanId}, RemainingPosts={RemainPosts}",
                        request.PostId, userPlan.Id, userPlan.RemainPosts);

                    isValid = true;
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error processing user plan: {ex.Message}";
                logger.LogError(ex, "Error processing user plan for PostId={PostId}", request.PostId);
            }

            if (isValid)
            {
                await eventPublisher.PublishAsync(new UserPlanValidEventModel
                {
                    PostId = request.PostId
                });
            }
            else
            {
                await eventPublisher.PublishAsync(new UserPlanInvalidEventModel
                {
                    PostId = request.PostId,
                    ErrorMessage = errorMessage
                });
            }
        }

        public async Task Handle(RollbackUserPlanEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received request to rollback user plan for PostId={PostId}, UserPlanId={UserPlanId}",
                request.PostId, request.UserPlanId);

            await ExecuteWithTransactionAsync(async () =>
            {
                var userPlan = await dbContext.UserPlans
                    .FirstOrDefaultAsync(up => up.Id == request.UserPlanId, cancellationToken);

                if (userPlan != null)
                {
                    // Increment the remaining posts count
                    userPlan.RemainPosts += 1;

                    // If the plan was deactivated due to zero posts, reactivate it
                    if (!userPlan.IsActive && userPlan.RemainPosts > 0 && userPlan.EndDate > DateTime.UtcNow)
                    {
                        userPlan.IsActive = true;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Successfully rolled back post count for UserPlanId={UserPlanId}, new RemainPosts={RemainPosts}",
                        userPlan.Id, userPlan.RemainPosts);
                }
                else
                {
                    logger.LogWarning("UserPlan with ID {UserPlanId} not found for rollback", request.UserPlanId);
                }
            }, cancellationToken);
        }
    }
}