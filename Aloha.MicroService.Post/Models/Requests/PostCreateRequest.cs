using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Aloha.PostService.Models.Requests
{
    public class PostCreateRequest
    {
        [Required(ErrorMessage = "UserPlanId is required")]
        public Guid UserPlanId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = default!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(3, ErrorMessage = "Currency code must be 3 characters")]
        public string? Currency { get; set; } = "VND";

        [Required(ErrorMessage = "CategoryId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive number")]
        public int CategoryId { get; set; }

        private List<int> _categoryPath = new();

        [Required(ErrorMessage = "CategoryPath is required")]
        [MinLength(1, ErrorMessage = "CategoryPath must contain at least one category")]
        [CategoryPath]
        public List<int> CategoryPath
        {
            get => _categoryPath;
            set => _categoryPath = value;
        }

        [FromForm(Name = "categoryPath")]
        public string? CategoryPathString
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        CategoryPath = value.Trim('[', ']')
                            .Split(',')
                            .Select(x => int.Parse(x.Trim()))
                            .Where(x => x > 0)
                            .ToList();
                    }
                    catch
                    {
                        CategoryPath = new List<int>();
                    }
                }
            }
        }

        [Required(ErrorMessage = "ProvinceCode is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ProvinceCode must be a positive number")]
        public int ProvinceCode { get; set; }

        [Required(ErrorMessage = "DistrictCode is required")]
        [Range(1, int.MaxValue, ErrorMessage = "DistrictCode must be a positive number")]
        public int DistrictCode { get; set; }

        [Required(ErrorMessage = "WardCode is required")]
        [Range(1, int.MaxValue, ErrorMessage = "WardCode must be a positive number")]
        public int WardCode { get; set; }

        [FromForm(Name = "attributes")]
        public string? AttributesString
        {
            get => null;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        Attributes = JsonDocument.Parse(value);
                    }
                    catch
                    {
                        Attributes = null;
                    }
                }
            }
        }

        public JsonDocument? Attributes { get; set; }

        public List<IFormFile>? Images { get; set; }
    }

    public class CategoryPathAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not List<int> categories)
                return new ValidationResult("CategoryPath must be a list of integers");

            if (!categories.Any())
                return new ValidationResult("CategoryPath must contain at least one category");

            if (categories.Any(x => x <= 0))
                return new ValidationResult("All categories must be positive numbers");

            return ValidationResult.Success;
        }
    }
}