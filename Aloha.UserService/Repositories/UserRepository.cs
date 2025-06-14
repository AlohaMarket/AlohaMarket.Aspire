using Aloha.UserService.Data;
using Aloha.UserService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloha.UserService.Repositories
{
    public class UserRepository(UserDbContext context) : IUserRepository
    {

        public async Task<User> CreateUserAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<User?> UserExistsByPhoneNumberAsync(string phoneNum)
        {
            if (string.IsNullOrEmpty(phoneNum))
                return null;

            return await context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNum && u.IsActive);
        }
    }
}
