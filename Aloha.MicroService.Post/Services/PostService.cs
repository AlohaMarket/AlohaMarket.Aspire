using Aloha.EventBus.Abstractions;
using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;
using Aloha.PostService.Repositories;
using Aloha.Shared.Exceptions;
using PostEntity = Aloha.PostService.Models.Entity.Post;

namespace Aloha.PostService.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PostService> _logger;

        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            IEventPublisher eventPublisher,
            ILogger<PostService> logger)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<PostResponse?> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post is null)
                return null;

            return _mapper.Map<PostResponse>(post);
        }

        public async Task<IEnumerable<PostResponse>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsAsync(int page, int pageSize, string? searchTerm = null)
        {
            var pagedPosts = await _postRepository.GetPostsAsync(page, pageSize, searchTerm);
            return _mapper.Map<IEnumerable<PostResponse>>(pagedPosts);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsByUserIdAsync(Guid userId)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsByCategoryIdAsync(int categoryId)
        {
            var posts = await _postRepository.GetPostsByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null)
        {
            var posts = await _postRepository.GetPostsByLocationAsync(provinceCode, districtCode, wardCode);
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<PostResponse> CreatePostAsync(PostCreateRequest request)
        {
            try
            {
                // Map request to entity
                var post = _mapper.Map<PostEntity>(request);

                // Set initial validation flags to false
                post.IsLocationValid = false;
                post.IsCategoryValid = false;
                post.IsUserPlanValid = false;
                post.Status = PostStatus.PendingValidation;

                // Create post
                var createdPost = await _postRepository.CreatePostAsync(post);

                _logger.LogInformation("Created post with ID {PostId}", createdPost.Id);

                // Publish validation events
                await _eventPublisher.PublishAsync(new PostCreatedIntegrationEvent
                {
                    PostId = createdPost.Id,
                    UserId = createdPost.UserId,
                    UserPlanId = createdPost.UserPlanId,
                    CategoryId = createdPost.CategoryId,
                    CategoryPath = createdPost.CategoryPath,
                    ProvinceCode = createdPost.ProvinceCode,
                    DistrictCode = createdPost.DistrictCode,
                    WardCode = createdPost.WardCode
                });

                _logger.LogInformation(
                    "Published validation events for post ID {PostId}, location: {Province}/{District}/{Ward}, category path: {CategoryPath}",
                    createdPost.Id, createdPost.ProvinceCode, createdPost.DistrictCode, createdPost.WardCode,
                    string.Join("/", createdPost.CategoryPath));

                return _mapper.Map<PostResponse>(createdPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post: {ErrorMessage}", ex.Message);
                throw new BadRequestException($"Error creating post: {ex.Message}");
            }
        }

        public async Task<PostResponse> UpdatePostAsync(Guid postId, PostUpdateRequest request)
        {
            var existingPost = await _postRepository.GetPostByIdAsync(postId);
            if (existingPost is null)
                throw new NotFoundException($"Post with ID {postId} not found");

            // Update post properties
            _mapper.Map(request, existingPost);
            existingPost.UpdatedAt = DateTime.UtcNow;

            var updatedPost = await _postRepository.UpdatePostAsync(existingPost);

            _logger.LogInformation("Updated post with ID {PostId}", updatedPost.Id);

            return _mapper.Map<PostResponse>(updatedPost);
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var exists = await _postRepository.PostExistsAsync(postId);
            if (!exists)
                throw new NotFoundException($"Post with ID {postId} not found");

            var result = await _postRepository.DeletePostAsync(postId);

            if (result)
                _logger.LogInformation("Deleted post with ID {PostId}", postId);

            return result;
        }

        public async Task<PostResponse?> UpdatePostStatusAsync(Guid postId, PostStatus status)
        {
            var post = await _postRepository.UpdatePostStatusAsync(postId, status);
            if (post is null)
                return null;

            _logger.LogInformation("Updated post {PostId} status to {Status}", postId, status);

            return _mapper.Map<PostResponse>(post);
        }

        public async Task<PostResponse?> ActivatePostAsync(Guid postId, bool isActive)
        {
            var post = await _postRepository.ActivatePostAsync(postId, isActive);
            if (post is null)
                return null;

            _logger.LogInformation("Post {PostId} activation set to {IsActive}", postId, isActive);

            return _mapper.Map<PostResponse>(post);
        }

        public async Task<PostResponse?> PushPostAsync(Guid postId)
        {
            var post = await _postRepository.PushPostAsync(postId);
            if (post is null)
                return null;

            // Publish push event
            await _eventPublisher.PublishAsync(new PostPushIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId,
                UserPlanId = post.UserPlanId,
            });

            _logger.LogInformation("Pushed post with ID {PostId}", postId);

            return _mapper.Map<PostResponse>(post);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsForModerationAsync()
        {
            var posts = await _postRepository.GetPostsForModerationAsync();
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<IEnumerable<PostResponse>> GetPostsByStatusAsync(PostStatus status)
        {
            var posts = await _postRepository.GetPostsByStatusAsync(status);
            return _mapper.Map<IEnumerable<PostResponse>>(posts);
        }

        public async Task<bool> PostExistsAsync(Guid postId)
        {
            return await _postRepository.PostExistsAsync(postId);
        }
    }
}
