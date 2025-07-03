using Aloha.MicroService.Post.Models.Enums;
using Aloha.MicroService.Post.Models.Responses;
using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;
using Aloha.Shared.Meta;

namespace Aloha.PostService.Services
{
    public interface IPostService
    {
        Task<PostDetailResponse?> GetPostByIdAsync(Guid postId);
        Task<PagedData<PostListResponse>> GetSearchPostsAsync(string? searchTerm = null, int? locationId = null, LocationLevel? locationLevel = null, int? categoryId = null,
        int? minPrice = null, int? maxPrice = null, SortBy? sortBy = null, SortDirection? order = null, int page = 1, int pageSize = 10);
        Task<PagedData<PostListResponse>> GetPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<PostCreateResponse> CreatePostAsync(Guid userId, PostCreateRequest request);
        Task<PostCreateResponse> UpdatePostAsync(Guid postId, PostUpdateRequest request);
        Task<bool> DeletePostAsync(Guid postId);
        Task<PostCreateResponse?> UpdatePostStatusAsync(Guid postId, PostStatus status);
        Task<PostCreateResponse?> ActivatePostAsync(Guid postId, bool isActive);
        Task<PostCreateResponse?> PushPostAsync(Guid postId);
        Task<IEnumerable<PostCreateResponse>> GetPostsForModerationAsync();
        Task<IEnumerable<PostCreateResponse>> GetPostsByStatusAsync(PostStatus status);
        Task<bool> PostExistsAsync(Guid postId);
    }
}
