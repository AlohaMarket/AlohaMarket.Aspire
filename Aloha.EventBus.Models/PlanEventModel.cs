using Aloha.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloha.EventBus.Models
{
    public class UserPlanProvisioningResultEvent : IntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? UserPlanId { get; set; }
    }
}
