using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloha.MicroService.Plan.Models.Entities
{
    public class UserPlan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [ForeignKey("Plan")]
        public int PlanId { get; set; }
        public Plans Plan { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int UsedPosts { get; set; } = 0;

        [Required]
        public int UsedPushes { get; set; } = 0;

        [Required]
        public int MaxPush { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
