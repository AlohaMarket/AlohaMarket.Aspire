namespace Aloha.MicroService.Payment.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateAsync(Payments payment) =>
            await _repository.CreateAsync(payment);

        public async Task<Payments?> GetByIdAsync(string id) =>
            await _repository.GetByIdAsync(id);

        public async Task<List<Payments>> GetByUserIdAsync(string userId)
        {
            var allPayments = await _repository.GetAllAsync();
            return allPayments.Where(p => p.UserId == userId).ToList();
        }
    }
}
