using Aloha.CategoryService.Models.Entities;
using Aloha.CategoryService.Models.Requests;

namespace Aloha.CategoryService.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(Guid id);
        Task<Category> CreateCategoryAsync(AddCategoryRequest category);
        Task<Category> UpdateCategoryAsync(Guid id, UpdateCategoryRequest category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(Guid? parentId);
        Task<IEnumerable<Guid>> GetCategoryPath(Guid id);
    }
}
