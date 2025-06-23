using Aloha.CategoryService.Models.Entities;
using Aloha.CategoryService.Models.Requests;
using Aloha.CategoryService.Models.Responses;
using Aloha.CategoryService.Repositories;
using Aloha.Shared.Exceptions;
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

        public async Task<bool> DeleteCategoryAsync(int id)
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

        public async Task<IEnumerable<ViewCategoryResponse>> GetCategoriesAsync()
        {
            var categories = await repo.GetAllCategoriesAsync();
            return mapper.Map<IEnumerable<ViewCategoryResponse>>(categories);
        }

        public async Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(int parentId)
        {
            var categoryList = await repo.GetCategoriesByParentIdAsync(parentId);
            return categoryList;
        }

        public async Task<ViewCategoryResponse> GetCategoryByIdAsync(int id)
        {
            var category = await repo.GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }
            return mapper.Map<ViewCategoryResponse>(category);
        }

        public async Task<IEnumerable<int>> GetCategoryPath(int id)
        {
            var path = new List<int>();
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

        public async Task<Category> UpdateCategoryAsync(int id, UpdateCategoryRequest categoryRequest)
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

        public async Task<bool> IsValidCategoryPath(List<int> categoryPath)
        {
            for (int i = 0; i < categoryPath.Count; i++)
            {
                var category = await repo.GetCategoryByIdAsync(categoryPath[i]);
                if (category == null || !category.IsActive)
                {
                    return false;
                }

                // Check if the next categoryId in the path is a child of the current category (except for the leaf category)  
                if (i < categoryPath.Count - 1)
                {
                    var nextCategoryId = categoryPath[i + 1];
                    if (!category.Children.Any(child => child.Id == nextCategoryId))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
