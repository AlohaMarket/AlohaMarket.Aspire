using System.ComponentModel.DataAnnotations;

namespace Aloha.NotificationService.Models.DTOs
{
    public class CreateMessageDto
    {
        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string MessageType { get; set; } = "text";
    }
} 