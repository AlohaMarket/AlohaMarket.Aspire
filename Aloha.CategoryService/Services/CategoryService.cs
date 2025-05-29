using Aloha.CategoryService.Models.Entities;
using Aloha.CategoryService.Repositories;
using Aloha.ServiceDefaults.Exceptions;
using AutoMapper;

namespace Aloha.CategoryService.Services
{
    public class CategoryService(ICategoryRepository repo, IMapper mapper) : ICategoryService
    {
        public Task<Category> CreateCategoryAsync(Category category)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCategoryAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return repo.GetAllCategoriesAsync();
        }

        public Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(Guid? parentId)
        {
            throw new NotImplementedException();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            var category = await repo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }
            return category;
        }

        public Task<Category> UpdateCategoryAsync(Category category)
        {
            throw new NotImplementedException();
        }
    }
}
