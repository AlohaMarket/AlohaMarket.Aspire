using Aloha.PostService.Models.Entity;
using System.Text.Json;

namespace Aloha.PostService.Models.Responses
{
    public class PostCreateResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid UserPlanId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "VND";
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];
        public int ProvinceCode { get; set; }
        public string? ProvinceText { get; set; }
        public int DistrictCode { get; set; }
        public string? DistrictText { get; set; }
        public int WardCode { get; set; }
        public string? WardText { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public bool IsViolation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PushedAt { get; set; }
        public JsonDocument? Attributes { get; set; }
        public bool IsLocationValid { get; set; }
        public bool IsCategoryValid { get; set; }
        public bool IsUserPlanValid { get; set; }
        public PostStatus Status { get; set; }
        public string? LocationValidationMessage { get; set; }
        public string? CategoryValidationMessage { get; set; }
        public string? UserPlanValidationMessage { get; set; }
        public List<PostImageResponse> Images { get; set; } = new();
    }
}
