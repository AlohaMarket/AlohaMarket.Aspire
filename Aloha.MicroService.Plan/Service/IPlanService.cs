
using Aloha.MicroService.Plan.Models.Request;
using Aloha.MicroService.Plan.Models.Response;

namespace Aloha.MicroService.Plan.Service
{
    public interface IPlanService
    {
        // Plan operations
        Task<PlanResponse> GetPlanAsync(int id);
        Task<List<PlanResponse>> GetAllPlansAsync();
        Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request);
        Task<PlanResponse> UpdatePlanAsync(int id, UpdatePlanRequest request);
        Task DeletePlanAsync(int id);

        // UserPlan operations
        Task<List<UserPlanResponse>> GetUserPlansAsync(Guid userId);
        Task<UserPlanResponse> SubscribeUserToPlanAsync(Guid userId, int planId);
    }
}
