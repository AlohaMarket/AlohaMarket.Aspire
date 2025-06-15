using Aloha.MicroService.Plan.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.Data
{
    public class PlanDbContext : DbContext
    {
        public PlanDbContext(DbContextOptions<PlanDbContext> options) : base(options)
        {
        }

        public DbSet<Plans> Plans { get; set; }
        public DbSet<UserPlan> UserPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            modelBuilder.Entity<Plans>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
                entity.Property(p => p.Price).HasColumnType("numeric");
                entity.Property(p => p.CreateAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<UserPlan>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.Property(up => up.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(up => up.CreateAt).HasDefaultValueSql("NOW()");
                entity.HasOne(up => up.Plan)
                      .WithMany(p => p.UserPlans)
                      .HasForeignKey(up => up.PlanId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
