using Aloha.CategoryService.Models.Requests;
using Aloha.CategoryService.Services;
using Aloha.ServiceDefaults.Extensions;
using Aloha.ServiceDefaults.Meta;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.CategoryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryService.GetCategoriesAsync();
            return Ok(ApiResponseBuilder.BuildResponse(message: "categories retrieve successfully!", categories));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            return Ok(ApiResponseBuilder.BuildResponse(message: "category retrieve successfully", category));
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateCategory([FromBody] AddCategoryRequest request)
        {
            var category = await categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id },
                ApiResponseBuilder.BuildResponse(message: "category created successfully", category));
        }

        [HttpPut("{id:int}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await categoryService.UpdateCategoryAsync(id, request);
            return Ok(ApiResponseBuilder.BuildResponse(message: "category updated successfully", category));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await categoryService.DeleteCategoryAsync(id);
            return Ok(ApiResponseBuilder.BuildResponse(message: "category delete successfully", result));
        }

        [HttpGet("{id:int}/category_path")]
        public async Task<IActionResult> GetCategoryPath(int id)
        {
            var path = await categoryService.GetCategoryPath(id);
            return Ok(ApiResponseBuilder.BuildResponse(message: "category path retrieve successfully", path));
        }

        [HttpGet("parent/{parentId:int}")]
        public async Task<IActionResult> GetCategoriesByParentId(int parentId)
        {
            var categories = await categoryService.GetCategoriesByParentIdAsync(parentId);
            return Ok(ApiResponseBuilder.BuildResponse(message: "categories by parent retrieve successfully",
                categories));
        }
    }
}
