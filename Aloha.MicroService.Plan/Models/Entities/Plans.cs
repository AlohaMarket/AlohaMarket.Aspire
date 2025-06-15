using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloha.MicroService.Plan.Models.Entities
{
    public class Plans
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "numeric")]
        public decimal Price { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public int MaxPosts { get; set; }

        [Required]
        public int MaxPushes { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserPlan> UserPlans { get; set; }
    }
}
