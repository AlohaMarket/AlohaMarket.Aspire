using Aloha.ServiceDefaults.Exceptions;
using Aloha.UserService.Models.Entities;
using Aloha.UserService.Models.Requests;
using Aloha.UserService.Repositories;
using AutoMapper;

namespace Aloha.UserService.Services
{
    public class UserService(IUserRepository userRepository, IMapper mapper) : IUserService
    {

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            var user = mapper.Map<User>(request);
            var isExist = await userRepository.UserExistsAsync(user.Id);
            if (isExist)
            {
                throw new InvalidOperationException("User already exists.");
            }
            return await userRepository.CreateUserAsync(user);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.IsActive = false; // Soft delete
            var result = await userRepository.UpdateUserAsync(user);
            return true;
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return userRepository.GetAllUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with id {userId} not found.");
            }
            if (user.IsActive is false)
            {
                throw new InvalidOperationException($"User with id {userId} is not active.");
            }
            return user;
        }

        public async Task<User> UpdateUserAsync(UpdateUserRequest request)
        {
            var existingUser = await userRepository.GetUserByIdAsync(request.Id);
            if (existingUser == null)
            {
                throw new NotFoundException($"User with id {request.Id} not found.");
            }

            // Check if phone number has changed
            if (request.PhoneNumber != existingUser.PhoneNumber)
            {
                var userWithSamePhone = await userRepository.UserExistsByPhoneNumberAsync(request.PhoneNumber);

                // If a user with this phone exists and it's not the same user we're updating
                if (userWithSamePhone != null && userWithSamePhone.Id != request.Id)
                {
                    throw new InvalidOperationException($"Phone number {request.PhoneNumber} is already in use by another user.");
                }
            }

            // Proceed with the update if validation passes
            mapper.Map(request, existingUser);
            existingUser.UpdatedAt = DateTime.UtcNow; // Update the timestamp
            existingUser.IsActive = true; // Ensure the user is active
            return await userRepository.UpdateUserAsync(existingUser);
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await userRepository.UserExistsAsync(userId);
        }
    }
}
