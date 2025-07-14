using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.UserService.Services;
using MediatR;

namespace Aloha.UserService.EventHandler
{
    public class UserIntegrationEventHandlers :
        IRequestHandler<TestSendEventModel>,
        IRequestHandler<UserChatRequestEventModel>
    {
        private readonly ILogger<UserIntegrationEventHandlers> _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserService _userService;

        public UserIntegrationEventHandlers(
            ILogger<UserIntegrationEventHandlers> logger,
            IEventPublisher eventPublisher,
            IUserService userService)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
            _userService = userService;
        }

        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);

            _eventPublisher.PublishAsync(new TestReceiveEventModel
            {
                Message = "string 2",
                FromService = "UserService",
                ToService = "PostService"
            });
            return Task.CompletedTask;
        }

        public async Task Handle(UserChatRequestEventModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UserChatRequestEvent for UserId: {UserId} from {Service}",
                request.UserId, request.RequestingService);

            try
            {
                // Try to parse the string userId to Guid
                if (Guid.TryParse(request.UserId, out var userId))
                {
                    // Get user data from service
                    var user = await _userService.GetUserByIdAsync(userId);

                    // Send the response event
                    await _eventPublisher.PublishAsync(new UserProfileResponseEventModel
                    {
                        UserId = request.UserId,
                        UserName = user.UserName,
                        Email = user.Email ?? "",
                        AvatarUrl = user.AvatarUrl ?? "",
                        IsActive = user.IsActive,
                    });

                    _logger.LogInformation("Published UserProfileResponseEvent for UserId: {UserId}", request.UserId);
                }
                else
                {
                    _logger.LogWarning("Invalid user ID format: {UserId}", request.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserProfileRequestEvent for UserId: {UserId}", request.UserId);
            }

            return;
        }
    }
}