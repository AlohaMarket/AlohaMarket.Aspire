namespace Aloha.MicroService.Payment.Repositories
{
    public interface IPaymentRepository
    {
        /// <summary>
/// Asynchronously retrieves all payment records from the database.
/// </summary>
/// <returns>A list of all payment records.</returns>
Task<List<Payments>> GetAllAsync();
        /// <summary>
/// Retrieves a payment record by its unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the payment.</param>
/// <returns>The payment record if found; otherwise, null.</returns>
Task<Payments?> GetByIdAsync(string id);
        /// <summary>
/// Asynchronously inserts a new payment record into the database.
/// </summary>
///
/// <param name="payment">The payment entity to be created.</param>
Task CreateAsync(Payments payment);
        /// <summary>
/// Deletes a payment record by its identifier.
/// </summary>
/// <param name="id">The unique identifier of the payment to delete.</param>
/// <returns>True if a payment was deleted; otherwise, false.</returns>
Task<bool> DeleteAsync(string id);
    }
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payments> _collection;

        /// <summary>
        /// Initializes a new instance of the PaymentRepository using the provided MongoDB settings.
        /// </summary>
        /// <param name="options">MongoDB connection and collection settings.</param>
        public PaymentRepository(IOptions<MongoSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var db = client.GetDatabase(options.Value.DatabaseName);
            _collection = db.GetCollection<Payments>(options.Value.CollectionName);
        }

        /// <summary>
            /// Asynchronously retrieves all payment records from the database.
            /// </summary>
            /// <returns>A list of all payment records.</returns>
            public async Task<List<Payments>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        /// <summary>
            /// Retrieves a payment record by its unique identifier.
            /// </summary>
            /// <param name="id">The unique identifier of the payment.</param>
            /// <returns>The payment record if found; otherwise, null.</returns>
            public async Task<Payments?> GetByIdAsync(string id) =>
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

        /// <summary>
            /// Inserts a new payment record into the database asynchronously.
            /// </summary>
            public async Task CreateAsync(Payments payment) =>
            await _collection.InsertOneAsync(payment);

        /// <summary>
        /// Deletes a payment record by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the payment to delete.</param>
        /// <returns>True if a payment was deleted; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
