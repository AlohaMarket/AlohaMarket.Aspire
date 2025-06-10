using Aloha.ServiceDefaults.Extensions;
using Aloha.ServiceDefaults.Meta;
using Aloha.UserService.Models.Requests;
using Aloha.UserService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.UserService.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "ALOHA_ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(ApiResponseBuilder.BuildResponse(data: users, message: "Get All Users Successfully"));
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await userService.GetUserByIdAsync(userId);
            return Ok(ApiResponseBuilder.BuildResponse(data: user, message: "Get User Successfully"));
        }

        [HttpPost("register")]
        [ValidateModelAttribute]
        [Authorize] // Add this attribute to require authentication
        public async Task<IActionResult> CreateUser()
        {
            logger.LogInformation("Log from the CreateUser method in UserController");
            logger.LogInformation("---- JWT TOKEN DEBUGGING ----");
            logger.LogInformation("Is authenticated: {IsAuthenticated}", User?.Identity?.IsAuthenticated);
            logger.LogInformation("Claims count: {ClaimsCount}", User?.Claims?.Count());
            foreach (var claim in User?.Claims)
            {
                logger.LogInformation("CLAIM: {Type} = {Value}", claim.Type, claim.Value);
            }

            var request = new CreateUserRequest
            {
                Id = Guid.Parse(User.GetUserId()),
                UserName = User.GetUserName()
            };
            var user = await userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id },
                ApiResponseBuilder.BuildResponse(message: "Create User Successfully", data: user));
        }

        [HttpPut]
        [ValidateModelAttribute]
        [Authorize(Roles = "ALOHA_ADMIN, ALOHA_USER")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var user = await userService.UpdateUserAsync(request);
            return Ok(ApiResponseBuilder.BuildResponse(message: "Update User Successfully", data: user));
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await userService.DeleteUserAsync(userId);
            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpGet("profile")]
        [Authorize(Roles = "ALOHA_USER")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = Guid.Parse(User.GetUserId());
            var user = await userService.GetUserByIdAsync(userId);
            return Ok(ApiResponseBuilder.BuildResponse(data: user, message: "Get User Profile Successfully"));
        }
    }
}
