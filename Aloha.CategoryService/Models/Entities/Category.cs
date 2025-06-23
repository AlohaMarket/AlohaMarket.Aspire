using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloha.CategoryService.Models.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? ParentId { get; set; } = 0;

        [ForeignKey(nameof(ParentId))]
        public Category? Parent { get; set; }

        public int Level { get; set; } = 1;

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
