using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class UserPlanValidationRequestModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public Guid UserPlanId { get; set; }
    }

    public class UserPlanValidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public int RemainingPosts { get; set; }
    }

    public class UserPlanInvalidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // This event is for compensation when other validations fail
    public class RollbackPostUsageEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public Guid UserPlanId { get; set; }
    }
}