using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDto>> GetUsersByIdsAsync(string[] userIds);
        Task<bool> UpdateUserOnlineStatusAsync(string userId, bool isOnline);
    }
} 