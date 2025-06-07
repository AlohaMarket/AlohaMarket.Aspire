using System.ComponentModel.DataAnnotations;

namespace Aloha.UserService.Models.Entities
{
    public class User
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [StringLength(100)]
        public required string UserName { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateOnly? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; }
    }
}
