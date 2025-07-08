using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Responses;

namespace Aloha.MicroService.Post.Models.Responses
{
    public class PostListResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = default!;
        public string? ProvinceText { get; set; }
        public string? DistrictText { get; set; }
        public string? WardText { get; set; }
        public PostImageResponse Image { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public PostStatus Status { get; set; }
    }
}