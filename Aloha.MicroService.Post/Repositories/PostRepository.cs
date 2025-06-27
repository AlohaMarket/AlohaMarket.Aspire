using Aloha.PostService.Data;
using Aloha.PostService.Models.Entity;

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
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _context.Posts.Include(p => p.Images).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
        {
            return await _context.Posts
                .Include(p => p.Images)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByCategoryIdAsync(int categoryId)
        {
            return await _context.Posts
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByLocationAsync(int provinceCode, int? districtCode = null, int? wardCode = null)
        {
            var query = _context.Posts
                .Include(p => p.Images)
                .Where(p => p.ProvinceCode == provinceCode);

            if (districtCode.HasValue)
                query = query.Where(p => p.DistrictCode == districtCode.Value);

            if (wardCode.HasValue)
                query = query.Where(p => p.WardCode == wardCode.Value);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
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

        public async Task<IEnumerable<Post>> GetPostsForModerationAsync()
        {
            return await _context.Posts
                .Include(p => p.Images)
                .Where(p => p.Status == PostStatus.PendingValidation || p.IsViolation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByStatusAsync(PostStatus status)
        {
            return await _context.Posts
                .Include(p => p.Images)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
