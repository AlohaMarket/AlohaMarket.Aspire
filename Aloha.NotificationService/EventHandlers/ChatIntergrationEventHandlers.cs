using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using Aloha.NotificationService.Models.DTOs;
using Aloha.NotificationService.Services;
using MediatR;

namespace Aloha.NotificationService.EventHandlers
{
    public class ChatIntergrationEventHandlers(
        ILogger<ChatIntergrationEventHandlers> logger,
        IUserProfileCache userCache) : IRequestHandler<UserProfileResponseEventModel>
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


    }
}