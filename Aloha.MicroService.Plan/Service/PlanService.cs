using Aloha.MicroService.Plan.Data;
using Aloha.MicroService.Plan.Models.Entities;
using Aloha.MicroService.Plan.Models.Request;
using Aloha.MicroService.Plan.Models.Response;
using Aloha.MicroService.Plan.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Aloha.MicroService.Plan.Service
{
    public class PlanService : IPlanService
    {
        private readonly PlanDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPlanRepository _planRepository;
        public PlanService(
           IPlanRepository planRepository,
           IMapper mapper,
           PlanDbContext dbContext)
        {
            _planRepository = planRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }
        // Implement methods for plan management here, e.g., GetPlans, CreatePlan, etc.

        public async Task<PlanResponse> GetPlanAsync(int id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan == null || !plan.IsActive)
            {
                throw new KeyNotFoundException("Plan not found");
            }

            return _mapper.Map<PlanResponse>(plan);
        }

        public async Task<List<PlanResponse>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetAllAsync();
            return _mapper.Map<List<PlanResponse>>(plans);
        }

        public async Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request)
        {
            var plan = _mapper.Map<Plans>(request);
            plan.IsActive = true;
            plan.CreateAt = DateTime.UtcNow;

            await _planRepository.AddAsync(plan);
            return _mapper.Map<PlanResponse>(plan);
        }

        public async Task<PlanResponse> UpdatePlanAsync(int id, UpdatePlanRequest request)
        {
            var existingPlan = await _planRepository.GetByIdAsync(id);
            if (existingPlan == null || !existingPlan.IsActive)
            {
                throw new KeyNotFoundException("Plan not found");
            }

            _mapper.Map(request, existingPlan);
            await _planRepository.UpdateAsync(existingPlan);

            return _mapper.Map<PlanResponse>(existingPlan);
        }

        public async Task DeletePlanAsync(int id)
        {
            await _planRepository.DeleteAsync(id);
        }

        // UserPlan operations
        public async Task<List<UserPlanResponse>> GetUserPlansAsync(Guid userId)
        {
            var userPlans = await _planRepository.GetUserPlansAsync(userId);
            return _mapper.Map<List<UserPlanResponse>>(userPlans);
        }

        public async Task<UserPlanResponse> SubscribeUserToPlanAsync(Guid userId, int planId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var plan = await _planRepository.GetByIdAsync(planId);
                    if (plan == null) throw new Exception("Plan not found");

                    var userPlan = new UserPlan
                    {
                        UserId = userId,
                        PlanId = planId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                        IsActive = true
                    };

                    await _dbContext.UserPlans.AddAsync(userPlan);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return _mapper.Map<UserPlanResponse>(userPlan);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
