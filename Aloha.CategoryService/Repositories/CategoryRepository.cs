using Aloha.CategoryService.Data;
using Aloha.CategoryService.Models.Entities;
using Aloha.ServiceDefaults.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aloha.CategoryService.Repositories
{
    public class CategoryRepository(CategoryDbContext context) : ICategoryRepository
    {
        public async Task AddCategoryAsync(Category category)
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            return await context.Categories.AnyAsync(c => c.Id == id);
        }

        public Task<int> CountCategoriesInSameLevel(int level)
        {
            if (level <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be greater than 0.");
            }
            return context.Categories.CountAsync(c => c.Level == level);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await context.Categories.AsNoTracking()
                .ToListAsync()
                .ContinueWith(task => task.Result.AsEnumerable());
        }

        public async Task<IEnumerable<Category>> GetCategoriesByParentIdAsync(Guid? parentId)
        {
            return await context.Categories
                .Where(c => c.ParentId == parentId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await context.Categories.FindAsync(id);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            context.Categories.Update(category);
            await context.SaveChangesAsync();
        }
    }
}
