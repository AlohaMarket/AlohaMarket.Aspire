using Aloha.NotificationService.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Aloha.NotificationService.Services
{
    public class MemoryPostInfoCache : IPostInfoCache
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryPostInfoCache> _logger;
        private readonly TimeSpan _absoluteTtl = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _slidingTtl = TimeSpan.FromMinutes(5);

        public MemoryPostInfoCache(IMemoryCache cache, ILogger<MemoryPostInfoCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<PostDto?> GetPostAsync(string postId)
        {
            var cacheKey = $"post:{postId}";
            
            if (_cache.TryGetValue(cacheKey, out PostDto? post))
            {
                _logger.LogDebug("Post {PostId} found in cache", postId);
                return Task.FromResult(post);
            }

            _logger.LogDebug("Post {PostId} not found in cache", postId);
            return Task.FromResult<PostDto?>(null);
        }

        public Task SetPostAsync(PostDto post)
        {
            var cacheKey = $"post:{post.Id}";
            
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _absoluteTtl,
                SlidingExpiration = _slidingTtl,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, post, cacheEntryOptions);
            _logger.LogDebug("Post {PostId} cached with TTL {AbsoluteTtl}min/{SlidingTtl}min", 
                post.Id, _absoluteTtl.TotalMinutes, _slidingTtl.TotalMinutes);
            
            return Task.CompletedTask;
        }

        public Task RemovePostAsync(string postId)
        {
            var cacheKey = $"post:{postId}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Post {PostId} removed from cache", postId);
            return Task.CompletedTask;
        }
    }
}