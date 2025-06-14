using Aloha.MicroService.Post.Infrastructure.Entity;
using System.Text.Json;

namespace Aloha.MicroService.Post.Infrastructure.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
        {
        }

        public DbSet<Entity.Post> Posts { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity.Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.IsActive).HasDefaultValue(false);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.CategoryPath)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                          v => JsonSerializer.Deserialize<List<int>>(v, new JsonSerializerOptions()) ?? new List<int>())
                      .HasColumnType("jsonb");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
