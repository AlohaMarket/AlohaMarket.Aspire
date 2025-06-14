namespace Aloha.MicroService.Payment.Repositories
{
    public interface IPaymentRepository
    {
        Task<List<Payments>> GetAllAsync();
        Task<Payments?> GetByIdAsync(string id);
        Task CreateAsync(Payments payment);
        Task<bool> DeleteAsync(string id);
    }
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payments> _collection;

        public PaymentRepository(IOptions<MongoSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var db = client.GetDatabase(options.Value.DatabaseName);
            _collection = db.GetCollection<Payments>(options.Value.CollectionName);
        }

        public async Task<List<Payments>> GetAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<Payments?> GetByIdAsync(string id) =>
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Payments payment) =>
            await _collection.InsertOneAsync(payment);

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
