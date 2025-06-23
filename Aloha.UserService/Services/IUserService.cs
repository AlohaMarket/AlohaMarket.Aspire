using Aloha.UserService.Models.Entities;
using Aloha.UserService.Models.Requests;

namespace Aloha.UserService.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User> UpdateUserAsync(UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> UserExistsAsync(Guid userId);
        Task<User> UploadUserAvatar(Guid userId, IFormFile file);
    }
}
