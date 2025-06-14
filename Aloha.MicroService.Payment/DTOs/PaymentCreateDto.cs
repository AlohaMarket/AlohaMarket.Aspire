namespace Aloha.MicroService.Payment.DTOs
{
    public class PaymentCreateDto
    {
        public string UserId { get; set; } = null!;
        public string PlanId { get; set; } = null!;
        public int Price { get; set; }
    }
}
