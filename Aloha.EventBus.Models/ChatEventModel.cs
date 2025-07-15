using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class UserChatRequestEventModel : IntegrationEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string RequestingService { get; set; } = "ChatService";
    }

    public class PostChatRequestEventModel : IntegrationEvent
    {
        public string PostId { get; set; } = string.Empty;
        public string RequestingService { get; set; } = "ChatService";
    }
}