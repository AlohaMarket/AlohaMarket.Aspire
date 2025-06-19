using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.LocationService.Services;
using MediatR;

namespace Aloha.LocationService.EventHandlers
{
    public class LocationIntegrationEventHandler(
        ILogger<LocationIntegrationEventHandler> logger,
        IEventPublisher eventPublisher, ILocationService service) :
        IRequestHandler<TestSendEventModel>,
        IRequestHandler<PostCreatedIntegrationEvent>
    {
        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To=LocationService",
                request.Message, request.FromService);

            eventPublisher.PublishAsync(new TestReceiveEventModel
            {
                Message = "string 4",
                FromService = "LocationService",
                ToService = "PostService"
            });
            return Task.CompletedTask;
        }

        public async Task Handle(PostCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating location for PostId={PostId}", request.PostId);

            var validationResult = await service.ValidateLocationPathWithText(
                request.ProvinceCode,
                request.DistrictCode,
                request.WardCode);

            if (validationResult.IsValid)
            {
                await eventPublisher.PublishAsync(new LocationValidEventModel
                {
                    PostId = request.PostId,
                    ProvinceText = validationResult.ProvinceText!,
                    DistrictText = validationResult.DistrictText!,
                    WardText = validationResult.WardText!
                });
                logger.LogInformation("Location validated successfully for PostId={PostId}", request.PostId);
            }
            else
            {
                await eventPublisher.PublishAsync(new LocationInvalidEventModel
                {
                    PostId = request.PostId,
                    ErrorMessage = "Invalid location path: Province, District or Ward not found or not related."
                });
                logger.LogWarning("Invalid location for PostId={PostId}", request.PostId);
            }
        }
    }
}
