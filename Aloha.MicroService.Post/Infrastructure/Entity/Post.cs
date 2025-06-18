namespace Aloha.MicroService.Post.Infrastructure.Entity
{
    public class Post
    {
        // identifier session
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }

        // post information
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }

        // category tree
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];

        // location information
        public int ProvinceCode { get; set; }
        public string ProvinceText { get; set; }
        public int DistrictCode { get; set; }
        public string DistrictText { get; set; }
        public int WardCode { get; set; }
        public string WardText { get; set; }

        // post status and attributes
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; }
        public bool IsViolation { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PushedAt { get; set; }
        public JsonDocument? Attributes { get; set; }

        //validation and status messages
        public bool IsLocationValid { get; set; }
        public bool IsCategoryValid { get; set; }
        public bool IsUserPlanValid { get; set; }
        public bool IsFullyValidated => IsLocationValid && IsCategoryValid && IsUserPlanValid;
        public PostStatus Status { get; set; }
        public string? LocationValidationMessage { get; set; }
        public string? CategoryValidationMessage { get; set; }
        public string? UserPlanValidationMessage { get; set; }
    }

    public enum PostStatus
    {
        PendingValidation,
        Validated,
        Invalid,
        Rejected,
        Archived,
        Deleted
    }
}
