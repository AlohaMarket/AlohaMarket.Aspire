using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly PlanDbContext _context;

        public PlanRepository(PlanDbContext context)
        {
            _context = context;
        }

        // Plan operations
        public async Task<Plans?> GetByIdAsync(int id)
        {
            return await _context.Plans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Plans>> GetAllAsync()
        {
            return await _context.Plans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(Plans plan)
        {
            await _context.Plans.AddAsync(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Plans plan)
        {
            _context.Plans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan != null)
            {
                plan.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Plans
                .AnyAsync(p => p.Id == id && p.IsActive);
        }

        // UserPlan operations
        public async Task<List<UserPlan>> GetUserPlansAsync(Guid userId)
        {
            return await _context.UserPlans
                .AsNoTracking()
                .Include(up => up.Plan)
                .Where(up => up.UserId == userId && up.IsActive)
                .OrderByDescending(up => up.StartDate)
                .ToListAsync();
        }

        public async Task AddUserPlanAsync(UserPlan userPlan)
        {
            await _context.UserPlans.AddAsync(userPlan);
            await _context.SaveChangesAsync();
        }
    }
}
