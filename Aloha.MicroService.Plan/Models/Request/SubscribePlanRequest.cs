using System.ComponentModel.DataAnnotations;

namespace Aloha.MicroService.Plan.Models.Request
{
    public class SubscribePlanRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Plan ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Plan ID")]
        public int PlanId { get; set; }
    }
}
