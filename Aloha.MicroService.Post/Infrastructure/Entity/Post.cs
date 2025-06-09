namespace Aloha.MicroService.Post.Infrastructure.Entity
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];
        public JsonDocument? LocationPath { get; set; }
        public bool IsActive { get; set; }
        public PostStatus Status { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PushedAt { get; set; }
        public JsonDocument? Attributes { get; set; }
        public string? StatusMessage { get; set; }
    }
}
