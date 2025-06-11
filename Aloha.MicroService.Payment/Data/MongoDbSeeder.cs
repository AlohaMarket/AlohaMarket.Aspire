using Aloha.MicroService.Payment.Models;

namespace Aloha.MicroService.Payment.Data
{
    public static class MongoDbSeeder
    {
        public static async Task Seed(IMongoCollection<Payments> collection)
        {
            var count = await collection.CountDocumentsAsync(_ => true);

            if (count == 0)
            {
                var seedPayments = new List<Payments>
                {
                    new Payments
                    {
                        UserId = Guid.NewGuid().ToString(),
                        PlanId = Guid.NewGuid().ToString(), 
                        Price = 200000,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Payments
                    {
                        UserId = Guid.NewGuid().ToString(),
                        PlanId = Guid.NewGuid().ToString(),
                        Price = 300000,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                    }
                };

                await collection.InsertManyAsync(seedPayments);
                Console.WriteLine("✅ Payment seed data inserted.");
            }
            else
            {
                Console.WriteLine("ℹ️ Payments collection already has data.");
            }
        }
    }
}
