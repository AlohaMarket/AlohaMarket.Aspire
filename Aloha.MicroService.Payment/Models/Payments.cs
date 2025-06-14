namespace Aloha.MicroService.Payment.Models
{
    public class Payments
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("user_id")]
        public string UserId { get; set; } = null!;

        [BsonElement("plan_id")]
        public string PlanId { get; set; } = null!;

        [BsonElement("price")]
        public int Price { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
