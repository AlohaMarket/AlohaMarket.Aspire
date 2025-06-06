using Aloha.CategoryService.Models.Entities;
using Aloha.CategoryService.Models.Requests;
using Aloha.CategoryService.Models.Responses;

namespace Aloha.CategoryService.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<ViewCategoryResponse>> GetCategoriesAsync();
        Task<ViewCategoryResponse> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(AddCategoryRequest category);
        Task<Category> UpdateCategoryAsync(int id, UpdateCategoryRequest category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(int parentId);
        Task<IEnumerable<int>> GetCategoryPath(int id);
    }
}
