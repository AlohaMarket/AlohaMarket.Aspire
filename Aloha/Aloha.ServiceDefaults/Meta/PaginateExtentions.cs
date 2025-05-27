using Microsoft.EntityFrameworkCore;

namespace Aloha.ServiceDefaults.Meta
{
    public static class PaginateExtensions
    {
        public static async Task<PagedData<T>> ToPaginatedResponse<T>(this IQueryable<T> query, int pageNumber, int pageSize, int firstPage = 1)
        {
            if (firstPage > pageNumber)
                throw new ArgumentException($"page ({pageNumber}) must greater or equal than firstPage ({firstPage})");

            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedData<T>
            {
                Items = items,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                }
            };
        }
    }
}
