using System.ComponentModel.DataAnnotations;

namespace Aloha.CategoryService.Models.Requests
{
    public class AddCategoryRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "SortOrder is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder must be a non-negative integer.")]
        public int SortOrder { get; set; }
    }
}
