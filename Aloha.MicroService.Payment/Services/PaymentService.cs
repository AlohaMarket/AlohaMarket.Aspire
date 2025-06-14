namespace Aloha.MicroService.Payment.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _repository;

        /// <summary>
        /// Initializes a new instance of the PaymentService with the specified payment repository.
        /// </summary>
        public PaymentService(IPaymentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
            /// Asynchronously creates a new payment record.
            /// </summary>
            public async Task CreateAsync(Payments payment) =>
            await _repository.CreateAsync(payment);

        /// <summary>
            /// Asynchronously retrieves a payment by its unique identifier.
            /// </summary>
            /// <param name="id">The unique identifier of the payment.</param>
            /// <returns>The payment with the specified ID, or null if not found.</returns>
            public async Task<Payments?> GetByIdAsync(string id) =>
            await _repository.GetByIdAsync(id);

        /// <summary>
        /// Asynchronously retrieves all payments associated with the specified user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose payments are to be retrieved.</param>
        /// <returns>A list of payments belonging to the specified user.</returns>
        public async Task<List<Payments>> GetByUserIdAsync(string userId)
        {
            var allPayments = await _repository.GetAllAsync();
            return allPayments.Where(p => p.UserId == userId).ToList();
        }
    }
}
