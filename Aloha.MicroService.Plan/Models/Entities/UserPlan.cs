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

        [Required]
        public Plans Plan { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int RemainPosts { get; set; }

        [Required]
        public int RemainPushes { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
