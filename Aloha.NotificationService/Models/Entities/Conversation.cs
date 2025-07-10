using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Aloha.NotificationService.Models.Entities
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("conversationType")]
        [Required]
        public string ConversationType { get; set; } // "buyer_seller" or "support"

        [BsonElement("productId")]
        public string ProductId { get; set; }

        [BsonElement("lastMessageAt")]
        public DateTime LastMessageAt { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        // Embedded participants instead of separate collection
        [BsonElement("participants")]
        public List<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

        // Embedded product context for performance
        [BsonElement("productContext")]
        public ProductContext ProductContext { get; set; }
    }
    public class ConversationParticipant
    {
        [BsonElement("userId")]
        [Required]
        public string UserId { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("userEmail")]
        public string UserEmail { get; set; }

        [BsonElement("userAvatar")]
        public string UserAvatar { get; set; }

        [BsonElement("joinedAt")]
        public DateTime JoinedAt { get; set; }

        [BsonElement("lastReadAt")]
        public DateTime LastReadAt { get; set; }

        [BsonElement("isOnline")]
        public bool IsOnline { get; set; }
    }

    public class ProductContext
    {
        [BsonElement("productId")]
        public string ProductId { get; set; }

        [BsonElement("productName")]
        public string ProductName { get; set; }

        [BsonElement("productImage")]
        public string ProductImage { get; set; }

        [BsonElement("productPrice")]
        public decimal ProductPrice { get; set; }

        [BsonElement("sellerId")]
        public string SellerId { get; set; }

        [BsonElement("sellerName")]
        public string SellerName { get; set; }
    }
}
