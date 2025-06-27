using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;

namespace Aloha.PostService.Services
{
    public interface IPostService
    {
        Task<PostResponse?> GetPostByIdAsync(Guid postId);
        Task<IEnumerable<PostResponse>> GetAllPostsAsync();
        Task<IEnumerable<PostResponse>> GetPostsAsync(int page, int pageSize, string? searchTerm = null);
        Task<IEnumerable<PostResponse>> GetPostsByUserIdAsync(Guid userId);
        Task<IEnumerable<PostResponse>> GetPostsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<PostResponse>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null);
        Task<PostResponse> CreatePostAsync(PostCreateRequest request);
        Task<PostResponse> UpdatePostAsync(Guid postId, PostUpdateRequest request);
        Task<bool> DeletePostAsync(Guid postId);
        Task<PostResponse?> UpdatePostStatusAsync(Guid postId, PostStatus status);
        Task<PostResponse?> ActivatePostAsync(Guid postId, bool isActive);
        Task<PostResponse?> PushPostAsync(Guid postId);
        Task<IEnumerable<PostResponse>> GetPostsForModerationAsync();
        Task<IEnumerable<PostResponse>> GetPostsByStatusAsync(PostStatus status);
        Task<bool> PostExistsAsync(Guid postId);
    }
}
