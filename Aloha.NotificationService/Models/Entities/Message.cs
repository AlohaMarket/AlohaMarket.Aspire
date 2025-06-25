using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Aloha.NotificationService.Models.Entities
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("conversationId")]
        [Required]
        public string ConversationId { get; set; }

        [BsonElement("senderId")]
        [Required]
        public string SenderId { get; set; }

        [BsonElement("senderName")]
        public string SenderName { get; set; }

        [BsonElement("senderAvatar")]
        public string SenderAvatar { get; set; }

        [BsonElement("content")]
        [Required]
        public string Content { get; set; }

        [BsonElement("messageType")]
        public string MessageType { get; set; } // "text", "image", "file"

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("isRead")]
        public bool IsRead { get; set; }

        [BsonElement("isEdited")]
        public bool IsEdited { get; set; }

        [BsonElement("editedAt")]
        public DateTime? EditedAt { get; set; }

        [BsonElement("replyToMessageId")]
        public string ReplyToMessageId { get; set; }

        // Embedded reply context for better performance
        [BsonElement("replyTo")]
        public MessageReplyContext ReplyTo { get; set; }

        // Read status by participants
        [BsonElement("readBy")]
        public List<MessageReadStatus> ReadBy { get; set; } = new List<MessageReadStatus>();
    }
    public class MessageReplyContext
    {
        [BsonElement("messageId")]
        public string MessageId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("senderName")]
        public string SenderName { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    public class MessageReadStatus
    {
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("readAt")]
        public DateTime ReadAt { get; set; }
    }
}
