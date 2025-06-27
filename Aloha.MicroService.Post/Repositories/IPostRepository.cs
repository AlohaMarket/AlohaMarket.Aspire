using Aloha.PostService.Models.Entity;

namespace Aloha.PostService.Repositories
{
    public interface IPostRepository
    {
        Task<Post?> GetPostByIdAsync(Guid postId);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<IEnumerable<Post>> GetPostsAsync(int page, int pageSize, string? searchTerm = null);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId);
        Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Post>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null);
        Task<Post> CreatePostAsync(Post post);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid postId);
        Task<bool> PostExistsAsync(Guid postId);
        Task<Post?> UpdatePostStatusAsync(Guid postId, PostStatus status);
        Task<Post?> ActivatePostAsync(Guid postId, bool isActive);
        Task<Post?> PushPostAsync(Guid postId);
        Task<IEnumerable<Post>> GetPostsForModerationAsync();
        Task<IEnumerable<Post>> GetPostsByStatusAsync(PostStatus status);
    }
}
