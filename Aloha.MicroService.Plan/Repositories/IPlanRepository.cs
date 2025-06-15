using Aloha.MicroService.Plan.Models.Entities;

namespace Aloha.MicroService.Plan.Repositories
{
    public interface IPlanRepository
    {
        Task<Plans?> GetByIdAsync(int id);
        Task<List<Plans>> GetAllAsync();
        Task AddAsync(Plans plan);
        Task UpdateAsync(Plans plan);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        
        Task<List<UserPlan>> GetUserPlansAsync(Guid userId);
        Task AddUserPlanAsync(UserPlan userPlan);
    }
}
