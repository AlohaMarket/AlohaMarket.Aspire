using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Models.Entities;
using Aloha.MicroService.Plan.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.EventHandlers
{
    public class PlanIntegrationEventHandler(
       ILogger<PlanIntegrationEventHandler> logger,
       IEventPublisher eventPublisher,
       PlanDbContext dbContext,
       IPlanRepository planRepository
   ) :
       IRequestHandler<CreateUserPlanCommand>
    {
        public async Task Handle(CreateUserPlanCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received CreateUserPlanCommand: PaymentId={PaymentId}, UserId={UserId}, PlanId={PlanId}, Amount={Amount}",
                request.PaymentId, request.UserId, request.PlanId, request.Amount);

            Guid? userPlanId = null;
            bool isSuccess = false;
            string message;

            try
            {

                var plan = await planRepository.GetByIdAsync(request.PlanId);
                if (plan == null)
                {
                    message = "Plan not found.";
                }
                else
                {
                    // Tạo user plan mới
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
                    logger.LogInformation("Before AddUserPlanAsync");
                    logger.LogInformation("Insert UserPlan with Id: {Id}", userPlan.Id);
                    await planRepository.AddUserPlanAsync(userPlan);
                    logger.LogInformation("After AddUserPlanAsync");
                    userPlanId = userPlan.Id;
                    isSuccess = true;
                    message = "User plan provisioned successfully.";
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key value violates unique constraint") == true)
            {
                // Trùng PK, truy vấn lại UserPlan đã tồn tại
                var existingUserPlan = await dbContext.UserPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(up =>
                        up.UserId == request.UserId &&
                        up.PlanId == request.PlanId);

                userPlanId = existingUserPlan?.Id;
                isSuccess = true;
                message = "User plan already exists.";
            }
            catch (Exception ex)
            {
                message = $"Exception: {ex.Message}";
                logger.LogError(ex, "Error provisioning user plan for PaymentId={PaymentId}. Inner: {Inner}", request.PaymentId, ex.InnerException?.Message);
            }

            // Gửi kết quả về Payment
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
    }
}