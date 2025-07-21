using Aloha.MicroService.Post.Models.Enums;
using Aloha.PostService.Data;
using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.PostService.Models.Responses;
using Aloha.Shared.Exceptions;
using Aloha.Shared.Meta;

namespace Aloha.PostService.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDbContext _context;

        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetPostByIdAsync(Guid postId)
        {
            return await _context.Posts
                .Include(p => p.Images).AsNoTracking()
                .Where(p => p.IsActive)
                .Where(p => p.Status == PostStatus.Validated)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<PagedData<Post>> GetPostsAsync(string? searchTerm = null,
            int? locationId = null, LocationLevel? locationLevel = null, int? categoryId = null,
            int? minPrice = null, int? maxPrice = null, SortBy? sortBy = null, SortDirection? order = null,
            int page = 1, int pageSize = 10)
        {
            var query = _context.Posts
                .Include(p => p.Images.Where(img => img.Order == 1))
                .AsNoTracking()
                .AsQueryable();

            query = query.Where(p => p.IsActive).Where(p => p.Status == PostStatus.Validated);

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (locationId.HasValue)
            {
                query = locationLevel switch
                {
                    LocationLevel.Province => query.Where(p => p.ProvinceCode == locationId.Value),
                    LocationLevel.District => query.Where(p => p.DistrictCode == locationId.Value),
                    LocationLevel.Ward => query.Where(p => p.WardCode == locationId.Value),
                    _ => throw new BadRequestException($"Invalid location level {locationLevel}")
                };
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => EF.Functions.JsonContains(p.CategoryPath, categoryId.Value.ToString()));
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm));
            }

            // Apply base ordering
            query = query.OrderByDescending(p => p.CreatedAt)
                        .ThenByDescending(p => p.Priority);

            // Apply custom sorting if specified
            if (sortBy.HasValue)
            {
                query = sortBy switch
                {
                    SortBy.Price => order == SortDirection.Asc
                        ? query.OrderBy(p => p.Price).ThenByDescending(p => p.Priority)
                        : query.OrderByDescending(p => p.Price).ThenByDescending(p => p.Priority),
                    SortBy.CreatedAt => order == SortDirection.Asc
                        ? query.OrderBy(p => p.CreatedAt).ThenByDescending(p => p.Priority)
                        : query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Priority),
                    _ => throw new BadRequestException($"Invalid sort by {sortBy}")
                };
            }

            var totalCount = await query.CountAsync();
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedData<Post>
            {
                Items = posts,
                Meta = new PaginationMeta
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }

        public async Task<PagedData<Post>> GetPostsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, PostStatus? postStatus = null)
        {
            var query = _context.Posts
                .Include(p => p.Images)
                .Where(p => p.UserId == userId)
                .Where(p => p.IsActive)
                .AsNoTracking()
                .AsQueryable();

            if (postStatus.HasValue)
                query = query.Where(p => p.Status == postStatus.Value);

            var totalCount = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.UpdatedAt)
                .ThenByDescending(p => p.Priority)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedData<Post>
            {
                Items = posts,
                Meta = new PaginationMeta
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            post.Id = Guid.NewGuid();
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            post.Status = PostStatus.PendingValidation;

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post> UpdatePostAsync(Post post)
        {
            post.UpdatedAt = DateTime.UtcNow;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PostExistsAsync(Guid postId)
        {
            return await _context.Posts.AnyAsync(p => p.Id == postId);
        }

        public async Task<Post?> UpdatePostStatusAsync(Guid postId, PostStatus status)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return null;

            post.Status = status;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post?> ActivatePostAsync(Guid postId, bool isActive)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return null;

            post.IsActive = isActive;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post?> PushPostAsync(Guid postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return null;

            post.PushedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<PagedData<Post>> GetPostsByStatusAsync(int page, int pageSize, PostStatus? status)
        {
            var query = _context.Posts
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PagedData<Post>
            {
                Items = posts,
                Meta = new PaginationMeta
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }

        public async Task<PagedData<Post>> GetViolationPostsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Posts
                .Include(p => p.Images)
                .Where(p => p.IsViolation == true)
                .Where(p => p.Status != PostStatus.Deleted && p.Status != PostStatus.Rejected)
                .AsNoTracking()
                .OrderByDescending(p => p.UpdatedAt);

            var totalCount = await query.CountAsync();
            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedData<Post>
            {
                Items = posts,
                Meta = new PaginationMeta
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }

        public async Task<Post?> RecoveryViolationPostAsync(Guid postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return null;

            if (!post.IsViolation)
                return null; // Post is not in violation status

            post.IsViolation = false;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<PostStatisticsResponse> GetPostStatisticsAsync()
        {
            var totalPosts = await _context.Posts.CountAsync();
            var pendingPosts = await _context.Posts.CountAsync(p => p.Status == PostStatus.PendingValidation);
            var validatedPosts = await _context.Posts.CountAsync(p => p.Status == PostStatus.Validated);
            var invalidPosts = await _context.Posts.CountAsync(p => p.Status == PostStatus.Invalid);
            var rejectedPosts = await _context.Posts.CountAsync(p => p.Status == PostStatus.Rejected);
            var archivedPosts = await _context.Posts.CountAsync(p => p.Status == PostStatus.Archived);
            var violationPosts = await _context.Posts.CountAsync(p => p.IsViolation == true && p.Status != PostStatus.Deleted && p.Status != PostStatus.Rejected);

            return new PostStatisticsResponse
            {
                Total = totalPosts,
                Pending = pendingPosts,
                Validated = validatedPosts,
                Invalid = invalidPosts,
                Rejected = rejectedPosts,
                Archived = archivedPosts,
                Violation = violationPosts
            };
        }
    }
}
