using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Aloha.MicroService.Post.Infrastructure.Request
{
    public class PostCreateRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid UserPlanId { get; set; }

        // post information
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }
        [Required]
        public string Currency { get; set; }

        // category tree
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive integer.")]
        public int CategoryId { get; set; }
        [Required]
        public List<int> CategoryPath { get; set; }

        // location information
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Province code must be a positive integer.")]
        public int ProvinceCode { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "District code must be a positive integer.")]
        public int DistrictCode { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Ward code must be a positive integer.")]
        public int WardCode { get; set; }

        // additional attributes
        public JsonDocument? Attributes { get; set; }

    }
}