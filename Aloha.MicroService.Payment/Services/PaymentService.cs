using Aloha.MicroService.Payment.Models;

namespace Aloha.MicroService.Payment.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _repo;

        public PaymentService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Payments>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Payments?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
        public Task CreateAsync(Payments payment) => _repo.CreateAsync(payment);
        public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
    }
}
