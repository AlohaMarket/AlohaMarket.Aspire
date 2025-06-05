using Aloha.NotificationService.Models.DTOs;
using System.Text.Json;

namespace Aloha.NotificationService.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly IConfiguration _configuration;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            
            // Configure base address from appsettings
            var productServiceUrl = _configuration["ServiceUrls:ProductService"];
            if (!string.IsNullOrEmpty(productServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(productServiceUrl);
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(string productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/products/{productId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ProductDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogWarning($"Failed to get product {productId}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product {productId}");
                return null;
            }
        }
    }
} 