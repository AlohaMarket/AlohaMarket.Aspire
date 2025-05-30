using Aloha.CategoryService.Models.Entities;
using Aloha.CategoryService.Models.Requests;
using Aloha.CategoryService.Repositories;
using Aloha.ServiceDefaults.Exceptions;
using AutoMapper;

namespace Aloha.CategoryService.Services
{
    public class CategoryService(ICategoryRepository repo, IMapper mapper) : ICategoryService
    {
        public async Task<Category> CreateCategoryAsync(AddCategoryRequest category)
        {
            var categoryEntity = mapper.Map<Category>(category);
            if (categoryEntity.ParentId.HasValue)
            {
                var parentCategory = await repo.GetCategoryByIdAsync(categoryEntity.ParentId.Value);
                if (parentCategory == null)
                {
                    throw new NotFoundException($"Parent category with ID {categoryEntity.ParentId.Value} not found.");
                }
                if (!parentCategory.IsActive)
                {
                    throw new InvalidOperationException($"Parent category with ID {categoryEntity.ParentId.Value} is not active.");
                }
                categoryEntity.Level = parentCategory.Level + 1;
                categoryEntity.SortOrder = parentCategory.Children.Count + 1; // Set sort order based on existing children
                await repo.AddCategoryAsync(categoryEntity);
            }
            else
            {
                categoryEntity.Level = 1; // Root category level
                categoryEntity.SortOrder = await repo.CountCategoriesInSameLevel(1) + 1; // Set sort order for root categories
                await repo.AddCategoryAsync(categoryEntity);
            }
            return categoryEntity;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await repo.GetCategoryByIdAsync(id);
            if (category is null)
            {
                throw new NotFoundException($"Category with id {id} not found.");
            }
            category.IsActive = false;
            await repo.UpdateCategoryAsync(category);
            return true;
        }

        public Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return repo.GetAllCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(Guid? parentId)
        {
            var categoryList = await repo.GetCategoriesByParentIdAsync(parentId);
            return categoryList;
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

        public async Task<IEnumerable<Guid>> GetCategoryPath(Guid id)
        {
            var path = new List<Guid>();
            var current = await repo.GetCategoryByIdAsync(id);
            if (current == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }
            while (current != null)
            {
                path.Add(current.Id);
                current = current.Parent;
            }
            return path;
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, UpdateCategoryRequest categoryRequest)
        {
            var existingCategory = await repo.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }
            existingCategory = mapper.Map(categoryRequest, existingCategory);
            await repo.UpdateCategoryAsync(existingCategory);
            return existingCategory;
        }
    }
}
