using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Aloha.PostService.Models.Requests
{
    public class PostUpdateRequest
    {
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string? Title { get; set; }

        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string? Currency { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive number")]
        public int? CategoryId { get; set; }

        [MinLength(1, ErrorMessage = "CategoryPath must contain at least one category")]
        public List<int>? CategoryPath { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProvinceCode must be a positive number")]
        public int? ProvinceCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "DistrictCode must be a positive number")]
        public int? DistrictCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "WardCode must be a positive number")]
        public int? WardCode { get; set; }

        public JsonDocument? Attributes { get; set; }
    }
}
