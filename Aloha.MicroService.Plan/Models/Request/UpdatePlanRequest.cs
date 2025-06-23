using System.ComponentModel.DataAnnotations;

namespace Aloha.MicroService.Plan.Models.Request
{
    public class UpdatePlanRequest
    {
        [Required(ErrorMessage = "Plan name is required")]
        [StringLength(100, ErrorMessage = "Plan name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Maximum posts is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum posts must be at least 1")]
        public int MaxPosts { get; set; }

        [Required(ErrorMessage = "Maximum pushes is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Maximum pushes cannot be negative")]
        public int MaxPushes { get; set; }

        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; set; }
    }
}
