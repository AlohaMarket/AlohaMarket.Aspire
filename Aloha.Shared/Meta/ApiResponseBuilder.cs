namespace Aloha.Shared.Meta
{
    public class ApiResponseBuilder
    {

        public static ApiResponse<T> BuildResponse<T>(string message, T? data)
        {
            return new ApiResponse<T>
            {
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<PagedData<T>> BuildPageResponse<T>(
            IEnumerable<T> items,
            int totalPages,
            int currentPage,
            int pageSize,
            long totalItems,
            string message)
        {
            var pagedData = new PagedData<T>
            {
                Items = items,
                Meta = new PaginationMeta()
                {
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };

            return new ApiResponse<PagedData<T>>
            {
                Message = message,
                Data = pagedData
            };
        }
    }
}
