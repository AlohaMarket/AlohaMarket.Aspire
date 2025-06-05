using Aloha.NotificationService.Models.DTOs;

namespace Aloha.NotificationService.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(string productId);
    }
} 