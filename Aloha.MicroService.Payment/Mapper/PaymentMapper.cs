namespace Aloha.MicroService.Payment.Mapper
{
        public class PaymentMapper : Profile
        {
            /// <summary>
            /// Configures object mappings between payment-related data transfer objects and models.
            /// </summary>
            public PaymentMapper()
            {
                CreateMap<PaymentCreateDto, Payments>();
                CreateMap<Payments, PaymentResponseModel>();
            }
        }
}
