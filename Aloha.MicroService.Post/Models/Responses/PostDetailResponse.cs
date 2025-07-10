using Aloha.PostService.Models.Responses;

namespace Aloha.MicroService.Post.Models.Responses
{
    public class PostDetailResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = default!;
        public int ProvinceCode { get; set; }
        public int DistrictCode { get; set; }
        public int WardCode { get; set; }
        public string? ProvinceText { get; set; }
        public string? DistrictText { get; set; }
        public string? WardText { get; set; }
        public DateTime CreatedAt { get; set; }
        public JsonDocument? Attributes { get; set; }
        public List<PostImageResponse> Images { get; set; } = new();
    }
}