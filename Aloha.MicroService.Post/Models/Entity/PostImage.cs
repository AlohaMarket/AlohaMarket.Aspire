using System.ComponentModel.DataAnnotations;

namespace Aloha.PostService.Models.Entity
{
    public class PostImage
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        [Required]
        public int Order { get; set; }
    }
}
