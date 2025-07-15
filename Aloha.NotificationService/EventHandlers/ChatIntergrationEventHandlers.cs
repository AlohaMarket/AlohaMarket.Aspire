using Aloha.EventBus.Models;
using Aloha.NotificationService.Models.DTOs;
using Aloha.NotificationService.Services;
using MediatR;

namespace Aloha.NotificationService.EventHandlers
{
    public class ChatIntergrationEventHandlers(
        ILogger<ChatIntergrationEventHandlers> logger,
        IUserProfileCache userCache, IPostInfoCache postCache) : IRequestHandler<UserProfileResponseEventModel>, IRequestHandler<PostInfoResponseEventModel>
    {
        public async Task Handle(UserProfileResponseEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received UserProfileResponseEvent for UserId: {UserId}", request.UserId);

            try
            {
                var userDto = new UserDto
                {
                    Id = request.UserId,
                    Name = request.UserName,
                    Avatar = request.AvatarUrl,
                    IsOnline = false // Default offline, will be updated by SignalR
                };

                await userCache.SetUserAsync(userDto);
                logger.LogInformation("User profile cached successfully for UserId: {UserId}, UserName: {UserName}",
                    request.UserId, request.UserName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error caching user profile for UserId: {UserId}", request.UserId);
            }
        }

        public async Task Handle(PostInfoResponseEventModel request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Received PostInfoResponseEvent for PostId: {PostId}", request.PostId);

            try
            {
                var postDto = new PostDto
                {
                    Id = request.PostId,
                    Title = request.Title,
                    Price = request.Price,
                    ThumbnailUrl = request.ThumbnailUrl,
                    Status = request.Status,
                    Currency = request.Currency
                };

                await postCache.SetPostAsync(postDto);
                logger.LogInformation("Post info cached successfully for PostId: {PostId}, Title: {Title}",
                    request.PostId, request.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error caching post info for PostId: {PostId}", request.PostId);
            }
        }


    }
}