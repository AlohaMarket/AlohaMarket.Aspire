using Microsoft.Extensions.Caching.Memory;
using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public class MemoryUserProfileCache : IUserProfileCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryUserProfileCache> _logger;
        private readonly TimeSpan _absoluteExpiration = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(10);

        public MemoryUserProfileCache(IMemoryCache memoryCache, ILogger<MemoryUserProfileCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<UserDto?> GetUserAsync(string userId)
        {
            if (_memoryCache.TryGetValue($"user_{userId}", out UserDto? user))
            {
                _logger.LogDebug("User {UserId} found in cache", userId);
                return Task.FromResult(user);
            }

            _logger.LogDebug("User {UserId} not found in cache", userId);
            return Task.FromResult<UserDto?>(null);
        }

        public Task SetUserAsync(UserDto user)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _absoluteExpiration,
                SlidingExpiration = _slidingExpiration,
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set($"user_{user.Id}", user, options);
            _logger.LogDebug("User {UserId} cached with name: {UserName}", user.Id, user.Name);
            return Task.CompletedTask;
        }

        public Task RemoveUserAsync(string userId)
        {
            _memoryCache.Remove($"user_{userId}");
            _logger.LogDebug("User {UserId} removed from cache", userId);
            return Task.CompletedTask;
        }
    }
}