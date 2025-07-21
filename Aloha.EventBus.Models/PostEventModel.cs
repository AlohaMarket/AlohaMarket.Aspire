using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class PostCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];
        public int ProvinceCode { get; set; }
        public int DistrictCode { get; set; }
        public int WardCode { get; set; }
    }

    public class PostUpdatedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];
        public int ProvinceCode { get; set; }
        public int DistrictCode { get; set; }
        public int WardCode { get; set; }
    }

    public class PostPushIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }
    }

    public class RollbackUserPlanEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserPlanId { get; set; }
    }

    public class PostInfoResponseEventModel : IntegrationEvent
    {
        public string PostId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public string Currency { get; set; } = "VND";
    }
}
