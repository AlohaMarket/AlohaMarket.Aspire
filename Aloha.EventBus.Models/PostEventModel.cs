using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public enum PostStatus
    {
        Draft,
        Pending,
        Created
    }

    public class PostCreatedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid? UserPlanId { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public List<int> CategoryPath { get; set; } = [];
        public JsonDocument LocationPath { get; set; } = default!;
        public bool IsActive { get; set; }
        public PostStatus Status { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PushedAt { get; set; }
        public JsonDocument Attributes { get; set; } = default!;
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
        public JsonDocument LocationPath { get; set; } = default!;
        public bool IsActive { get; set; }
        public PostStatus Status { get; set; }
        public int Priority { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PushedAt { get; set; }
        public JsonDocument Attributes { get; set; } = default!;
    }

    public class PostStatusChangedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public PostStatus PreviousStatus { get; set; }
        public PostStatus CurrentStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PostActivationChangedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PostPushedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public DateTime PushedAt { get; set; }
    }

    public class PostDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
    }

}
