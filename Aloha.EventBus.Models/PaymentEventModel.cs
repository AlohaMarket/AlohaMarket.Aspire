using Aloha.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloha.EventBus.Models
{
    public class CreateUserPlanCommand : IntegrationEvent
    {
        public Guid UserId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid PlanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
