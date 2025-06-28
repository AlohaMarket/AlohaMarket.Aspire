using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.Shared.Meta;

namespace Aloha.PostService.Repositories
{
    public interface IPostRepository
    {
        Task<Post?> GetPostByIdAsync(Guid postId);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<PagedData<Post>> GetPostsAsync(string? searchTerm = null,
            int? locationId = null, LocationLevel? locationLevel = null, int? categoryId = null,
            int page = 1, int pageSize = 10);
        Task<PagedData<Post>> GetPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedData<Post>> GetPostsByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 10);
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
