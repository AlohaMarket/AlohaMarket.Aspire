using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using MediatR;

namespace Aloha.MicroService.Payment.EventHandlers
{
    public class PaymentIntegrationEventHandler(
          ILogger<PaymentIntegrationEventHandler> logger
      ) :
          IRequestHandler<UserPlanProvisioningResultEvent>
    {
        public Task Handle(UserPlanProvisioningResultEvent request, CancellationToken cancellationToken)
        {
            if (request.IsSuccess)
            {
                logger.LogInformation("UserPlanProvisioningResultEvent: Success for PaymentId={PaymentId}, UserPlanId={UserPlanId}, Message={Message}",
                    request.PaymentId, request.UserPlanId, request.Message);
                // TODO: Update payment status, notify user, etc.
            }
            else
            {
                logger.LogWarning("UserPlanProvisioningResultEvent: Failed for PaymentId={PaymentId}, Message={Message}",
                    request.PaymentId, request.Message);
                // TODO: Handle failure (compensation, notify user, etc.)
            }
            return Task.CompletedTask;
        }
    }
}