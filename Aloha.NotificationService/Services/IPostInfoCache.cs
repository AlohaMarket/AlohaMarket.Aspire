using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public interface IPostInfoCache
    {
        Task<PostDto?> GetPostAsync(string postId);
        Task SetPostAsync(PostDto post);
        Task RemovePostAsync(string postId);
    }
}