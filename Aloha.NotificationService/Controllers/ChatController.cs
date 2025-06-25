using Microsoft.AspNetCore.Mvc;
using Aloha.NotificationService.Services;
using Aloha.NotificationService.Models.Entities;

namespace Aloha.NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetUserConversations([FromQuery] string? userId = null)
        {
            // For testing without auth, get userId from query parameter or header
            userId = userId ?? Request.Headers["UserId"].FirstOrDefault() ?? "test-user-1";
            
            var conversations = await _chatService.GetUserConversations(userId);
            return Ok(conversations);
        }

        [HttpPost("conversations")]
        public async Task<ActionResult<Conversation>> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var conversation = await _chatService.CreateOrGetConversation(request.UserIds);
            return Ok(conversation);
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<Message>>> GetConversationMessages(
            string conversationId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? userId = null)
        {
            // For testing without auth, get userId from query parameter or header
            userId = userId ?? Request.Headers["UserId"].FirstOrDefault() ?? "test-user-1";

            // For testing, we'll allow access to any conversation
            // var isParticipant = await _chatService.IsUserInConversation(userId, conversationId);
            // if (!isParticipant)
            // {
            //     return Forbid("You are not a participant in this conversation");
            // }

            var messages = await _chatService.GetConversationMessages(conversationId, page, pageSize);
            return Ok(messages);
        }

        [HttpGet("conversations/{conversationId}/unread-count")]
        public async Task<ActionResult<long>> GetUnreadMessageCount(string conversationId, [FromQuery] string? userId = null)
        {
            // For testing without auth, get userId from query parameter or header
            userId = userId ?? Request.Headers["UserId"].FirstOrDefault() ?? "test-user-1";

            var count = await _chatService.GetUnreadMessageCount(userId, conversationId);
            return Ok(count);
        }

        [HttpPost("messages/{messageId}/read")]
        public async Task<IActionResult> MarkMessageAsRead(string messageId, [FromQuery] string? userId = null)
        {
            // For testing without auth, get userId from query parameter or header
            userId = userId ?? Request.Headers["UserId"].FirstOrDefault() ?? "test-user-1";

            await _chatService.MarkMessagesAsRead(userId, new[] { messageId });
            return Ok();
        }

        [HttpPut("messages/{messageId}")]
        public async Task<ActionResult<Message>> EditMessage(string messageId, [FromBody] EditMessageRequest request)
        {
            var message = await _chatService.EditMessage(messageId, request.Content);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            await _chatService.DeleteMessage(messageId);
            return NoContent();
        }
    }

    public class CreateConversationRequest
    {
        public string[] UserIds { get; set; } = Array.Empty<string>();
        public string? ProductId { get; set; }
    }

    public class EditMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
} 