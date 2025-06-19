using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Repositories;
using MediatR;

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
                // Insert user_plan (giả lập, thay bằng logic thực tế)
                var userPlan = await planRepository.(request.UserId, request.PlanId, request.Amount, request.PaymentDate, request.PaymentId);
                userPlanId = userPlan?.Id;
                isSuccess = userPlan != null;
                message = isSuccess ? "User plan provisioned successfully." : "Failed to provision user plan.";
            }
            catch (Exception ex)
            {
                message = $"Exception: {ex.Message}";
                logger.LogError(ex, "Error provisioning user plan for PaymentId={PaymentId}", request.PaymentId);
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