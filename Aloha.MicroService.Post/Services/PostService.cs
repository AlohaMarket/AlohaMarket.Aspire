using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Models.Responses;
using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Models.Responses;
using Aloha.PostService.Repositories;
using Aloha.ServiceDefaults.Cloudinary;
using Aloha.Shared.Exceptions;
using Aloha.Shared.Meta;
using AutoMapper;
using PostEntity = Aloha.PostService.Models.Entity.Post;

namespace Aloha.PostService.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PostService> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            IEventPublisher eventPublisher,
            ILogger<PostService> logger,
            ICloudinaryService cloudinaryService)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PostDetailResponse?> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post is null)
                throw new NotFoundException($"Post with ID {postId} not found");

            return _mapper.Map<PostDetailResponse>(post);
        }

        public async Task<IEnumerable<PostListResponse>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return _mapper.Map<IEnumerable<PostListResponse>>(posts);
        }

        public async Task<PagedData<PostListResponse>> GetPostsAsync(string? searchTerm = null, int? locationId = null, LocationLevel? locationLevel = null, int? categoryId = null, int page = 1, int pageSize = 10)
        {
            if (page < 1)
                throw new BadRequestException("Page number must be greater than 0");

            if (locationId.HasValue && locationId < 0)
                throw new BadRequestException("Location ID must be a positive integer");

            if (locationLevel.HasValue && !Enum.IsDefined(typeof(LocationLevel), locationLevel.Value))
                throw new BadRequestException("Invalid location level");

            if (categoryId.HasValue && categoryId < 0)
                throw new BadRequestException("Category ID must be a positive integer");

            var pagedPosts = await _postRepository.GetPostsAsync(searchTerm, locationId, locationLevel, categoryId, page, pageSize);

            return new PagedData<PostListResponse>
            {
                Items = _mapper.Map<IEnumerable<PostListResponse>>(pagedPosts.Items),
                Meta = pagedPosts.Meta
            };
        }

        public async Task<PagedData<PostListResponse>> GetPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId, page, pageSize);
            return new PagedData<PostListResponse>
            {
                Items = _mapper.Map<IEnumerable<PostListResponse>>(posts.Items),
                Meta = posts.Meta
            };
        }

        public async Task<PagedData<PostListResponse>> GetPostsByCategoryIdAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetPostsByCategoryIdAsync(categoryId, page, pageSize);
            return new PagedData<PostListResponse>
            {
                Items = _mapper.Map<IEnumerable<PostListResponse>>(posts.Items),
                Meta = posts.Meta
            };
        }

        public async Task<PagedData<PostListResponse>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null)
        {
            var posts = await _postRepository.GetPostsByLocationAsync(provinceCode, districtCode, wardCode);
            return _mapper.Map<PagedData<PostListResponse>>(posts);
        }

        public async Task<PostCreateResponse> CreatePostAsync(Guid userId, PostCreateRequest request)
        {
            PostEntity? createdPost = null;
            List<string>? uploadedImageUrls = null;

            try
            {
                // Step 1: Upload images first (if any) with specific handling
                if (request.Images != null && request.Images.Any())
                {
                    _logger.LogInformation("Starting image upload for {ImageCount} images", request.Images.Count);

                    try
                    {
                        uploadedImageUrls = await _cloudinaryService.UploadImagesAsync(request.Images);
                        _logger.LogInformation("Successfully uploaded {UploadedCount} images", uploadedImageUrls.Count);
                    }
                    catch (Exception uploadEx)
                    {
                        _logger.LogError(uploadEx, "Unexpected error during image upload");
                        throw new BadRequestException($"Image upload failed: {uploadEx.Message}");
                    }
                }

                // Step 2: Map request to entity
                var post = _mapper.Map<PostEntity>(request);
                post.UserId = userId;

                // Set initial validation flags to false
                post.IsLocationValid = false;
                post.IsCategoryValid = false;
                post.IsUserPlanValid = false;
                post.Status = PostStatus.PendingValidation;

                // Step 3: Create post images if any were uploaded
                if (uploadedImageUrls != null && uploadedImageUrls.Any())
                {
                    post.Images = uploadedImageUrls.Select((url, index) => new PostImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = url,
                        Order = index + 1,
                        PostId = post.Id
                    }).ToList();
                }

                createdPost = await _postRepository.CreatePostAsync(post);
                _logger.LogInformation("Created post with ID {PostId}", createdPost.Id);


                try
                {
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
                }
                catch (Exception eventEx)
                {
                    _logger.LogError(eventEx, "Error publishing validation events for post ID {PostId}. Post was created but validation events failed.", createdPost.Id);
                }

                return _mapper.Map<PostCreateResponse>(createdPost);
            }
            catch (BadRequestException)
            {
                // Re-throw known exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating post: {ErrorMessage}", ex.Message);

                // If post was created but something failed afterward, we might want to clean up
                if (createdPost != null)
                {
                    try
                    {
                        await _postRepository.DeletePostAsync(createdPost.Id);
                        _logger.LogInformation("Cleaned up post {PostId} due to creation failure", createdPost.Id);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Failed to cleanup post {PostId} after creation failure", createdPost.Id);
                    }
                }

                throw new BadRequestException($"Error creating post: {ex.Message}");
            }
        }

        public async Task<PostCreateResponse> UpdatePostAsync(Guid postId, PostUpdateRequest request)
        {
            var existingPost = await _postRepository.GetPostByIdAsync(postId);
            if (existingPost is null)
                throw new NotFoundException($"Post with ID {postId} not found");

            // Update post properties
            _mapper.Map(request, existingPost);
            existingPost.UpdatedAt = DateTime.UtcNow;

            var updatedPost = await _postRepository.UpdatePostAsync(existingPost);

            _logger.LogInformation("Updated post with ID {PostId}", updatedPost.Id);

            return _mapper.Map<PostCreateResponse>(updatedPost);
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

        public async Task<PostCreateResponse?> UpdatePostStatusAsync(Guid postId, PostStatus status)
        {
            var post = await _postRepository.UpdatePostStatusAsync(postId, status);
            if (post is null)
                return null;

            _logger.LogInformation("Updated post {PostId} status to {Status}", postId, status);

            return _mapper.Map<PostCreateResponse>(post);
        }

        public async Task<PostCreateResponse?> ActivatePostAsync(Guid postId, bool isActive)
        {
            var post = await _postRepository.ActivatePostAsync(postId, isActive);
            if (post is null)
                return null;

            _logger.LogInformation("Post {PostId} activation set to {IsActive}", postId, isActive);

            return _mapper.Map<PostCreateResponse>(post);
        }

        public async Task<PostCreateResponse?> PushPostAsync(Guid postId)
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

            return _mapper.Map<PostCreateResponse>(post);
        }

        public async Task<IEnumerable<PostCreateResponse>> GetPostsForModerationAsync()
        {
            var posts = await _postRepository.GetPostsForModerationAsync();
            return _mapper.Map<IEnumerable<PostCreateResponse>>(posts);
        }

        public async Task<IEnumerable<PostCreateResponse>> GetPostsByStatusAsync(PostStatus status)
        {
            var posts = await _postRepository.GetPostsByStatusAsync(status);
            return _mapper.Map<IEnumerable<PostCreateResponse>>(posts);
        }

        public async Task<bool> PostExistsAsync(Guid postId)
        {
            return await _postRepository.PostExistsAsync(postId);
        }
    }
}
