using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class UserProfileResponseEventModel : IntegrationEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}