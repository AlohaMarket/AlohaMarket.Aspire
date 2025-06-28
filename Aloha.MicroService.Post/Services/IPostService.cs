using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;
using Aloha.Shared.Meta;

namespace Aloha.PostService.Services
{
    public interface IPostService
    {
        Task<PostResponse?> GetPostByIdAsync(Guid postId);
        Task<IEnumerable<PostResponse>> GetAllPostsAsync();
        Task<PagedData<PostResponse>> GetPostsAsync(string? searchTerm = null, int? locationId = null, LocationLevel? locationLevel = null, int? categoryId = null, int page = 1, int pageSize = 10);
        Task<PagedData<PostResponse>> GetPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PagedData<PostResponse>> GetPostsByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 10);
        Task<PagedData<PostResponse>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null);
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
