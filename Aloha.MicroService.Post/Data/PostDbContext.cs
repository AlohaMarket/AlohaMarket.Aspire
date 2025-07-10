using Aloha.PostService.Models.Entity;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Aloha.PostService.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

        public DbSet<Models.Entity.Post> Posts { get; set; } = default!;
        public DbSet<PostImage> PostImages { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("PostServiceDB");

            modelBuilder.Entity<Models.Entity.Post>(entity =>
            {
                // Primary key and ID generation
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                // Required fields
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.UserPlanId).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
                entity.Property(e => e.IsActive).HasDefaultValue(false);
                entity.Property(e => e.ProvinceText).IsRequired(false);
                entity.Property(e => e.DistrictText).IsRequired(false);
                entity.Property(e => e.WardText).IsRequired(false);

                // Enum as string
                entity.Property(e => e.Status).HasConversion<string>();

                // CategoryPath as jsonb with value comparer
                entity.Property(e => e.CategoryPath)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                        v => JsonSerializer.Deserialize<List<int>>(v, new JsonSerializerOptions()) ?? new List<int>())
                    .HasColumnType("jsonb")
                    .Metadata.SetValueComparer(new ValueComparer<List<int>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    ));

                entity.HasMany(p => p.Images)
                      .WithOne()
                      .HasForeignKey(img => img.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired();
                entity.Property(e => e.Order).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
