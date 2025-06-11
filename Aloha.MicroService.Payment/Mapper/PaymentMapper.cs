
namespace Aloha.MicroService.Payment.Mapper
{
    public static class PaymentMapper
    {
        public static Payments ToModel(PaymentCreateDto dto)
        {
            return new Payments
            {
                UserId = dto.UserId,
                PlanId = dto.PlanId,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
