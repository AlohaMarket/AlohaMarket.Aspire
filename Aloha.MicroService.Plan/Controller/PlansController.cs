using Aloha.MicroService.Plan.Models.Request;
using Aloha.MicroService.Plan.Models.Response;
using Aloha.MicroService.Plan.Service;
using Aloha.Security.Authorizations;
using Aloha.Shared.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.MicroService.Plan.Controller
{
    [ApiController]
    [Route("api/plan")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;
        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlanResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _planService.GetAllPlansAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlanResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPlanById(int id)
        {
            var plan = await _planService.GetPlanAsync(id);
            return Ok(plan);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PlanResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPlan = await _planService.CreatePlanAsync(request);
            return CreatedAtAction(nameof(GetPlanById), new { id = createdPlan.Id }, createdPlan);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PlanResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPlan = await _planService.UpdatePlanAsync(id, request);
            return Ok(updatedPlan);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeletePlan(int id)
        {
            await _planService.DeletePlanAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(List<UserPlanResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserPlans()
        {
            var userId = Guid.Parse(User.GetUserId());
            var userPlans = await _planService.GetUserPlansAsync(userId);
            return Ok(ApiResponseBuilder.BuildResponse("Get user plan success fully", userPlans));
        }

        [HttpPost("subscribe")]
        [ProducesResponseType(typeof(UserPlanResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SubscribeUserToPlan([FromBody] SubscribePlanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subscription = await _planService.SubscribeUserToPlanAsync(request.UserId, request.PlanId);
            return CreatedAtAction(nameof(GetUserPlans), new { userId = request.UserId }, subscription);
        }
        [HttpGet("user-plan/{userId:guid}")]
        [ProducesResponseType(typeof(List<UserPlanResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserPlansByUserId([FromRoute] Guid userId)
        {
            var userPlans = await _planService.GetUserPlansAsync(userId);
            if (userPlans == null || !userPlans.Any())
            {
                return NotFound();
            }
            return Ok(userPlans);
        }

        [HttpGet("all-userplans")]
        [ProducesResponseType(typeof(List<UserPlanResponse>), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "ALOHA_ADMIN")]
        public async Task<IActionResult> GetAllUserPlan(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var userPlans = await _planService.GetAllUserPlanAsync(startDate, endDate);
            return Ok(ApiResponseBuilder.BuildResponse("Get all users plan success fully", userPlans));
        }

    }
}
