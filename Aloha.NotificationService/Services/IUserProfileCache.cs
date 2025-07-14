using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public interface IUserProfileCache
    {
        Task<UserDto?> GetUserAsync(string userId);
        Task SetUserAsync(UserDto user);
        Task RemoveUserAsync(string userId);
    }
}