namespace Aloha.MicroService.Payment.Mapper
{
        public class PaymentMapper : Profile
        {
            public PaymentMapper()
            {
                CreateMap<PaymentCreateDto, Payments>();
                CreateMap<Payments, PaymentResponseModel>();
            }
        }
}
