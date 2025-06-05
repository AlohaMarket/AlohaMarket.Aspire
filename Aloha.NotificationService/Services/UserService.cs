using Aloha.NotificationService.Models.DTOs;
using System.Text.Json;

namespace Aloha.NotificationService.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(HttpClient httpClient, ILogger<UserService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            
            // Configure base address from appsettings
            var userServiceUrl = _configuration["ServiceUrls:UserService"];
            if (!string.IsNullOrEmpty(userServiceUrl))
            {
                _httpClient.BaseAddress = new Uri(userServiceUrl);
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                _logger.LogWarning($"Failed to get user {userId}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user {userId}");
                return null;
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByIdsAsync(string[] userIds)
        {
            try
            {
                var userIdQuery = string.Join(",", userIds);
                var response = await _httpClient.GetAsync($"/api/users/batch?ids={userIdQuery}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<IEnumerable<UserDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UserDto>();
                }
                
                _logger.LogWarning($"Failed to get users: {response.StatusCode}");
                return new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users batch");
                return new List<UserDto>();
            }
        }

        public async Task<bool> UpdateUserOnlineStatusAsync(string userId, bool isOnline)
        {
            try
            {
                var content = JsonContent.Create(new { isOnline });
                var response = await _httpClient.PutAsync($"/api/users/{userId}/online-status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating online status for user {userId}");
                return false;
            }
        }
    }
} 