using Aloha.CategoryService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloha.CategoryService.Data
{
    public class CategoryDbContext(DbContextOptions<CategoryDbContext> options) : DbContext(options)
    {
        public DbSet<Category> Categories { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("CategoryServiceDB");
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>(entity =>
                {
                    entity.HasIndex(c => c.ParentId);
                    entity.HasIndex(c => c.Level);
                    entity.HasIndex(c => c.SortOrder);

                    entity.HasOne(c => c.Parent)
                        .WithMany(c => c.Children)
                        .HasForeignKey(c => c.ParentId)
                        .OnDelete(DeleteBehavior.Restrict);
                }
            );
        }
    }
}
