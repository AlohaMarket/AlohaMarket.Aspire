using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloha.CategoryService.Models.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Category? Parent { get; set; }

        public int Level { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
