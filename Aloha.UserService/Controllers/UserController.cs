using Aloha.MicroService.User.Models.Responses;
using Aloha.Security.Authorizations;
using Aloha.Shared.Extensions;
using Aloha.Shared.Meta;
using Aloha.Shared.Validators;
using Aloha.UserService.Models.Requests;
using Aloha.UserService.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.UserService.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService, IMapper mapper) : ControllerBase
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
        [ValidateModel]
        [Authorize] // Add this attribute to require authentication
        public async Task<IActionResult> CreateUser()
        {
            var request = new CreateUserRequest
            {
                Id = Guid.Parse(User.GetUserId()),
                UserName = User.GetUserName()
            };
            var user = await userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id },
                ApiResponseBuilder.BuildResponse(message: "Create User Successfully", data: user));
        }

        [HttpPut("profile/update")]
        [ValidateModel]
        [Authorize(Roles = "ALOHA_ADMIN, ALOHA_USER")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            request.Id = Guid.Parse(User.GetUserId());
            var user = await userService.UpdateUserAsync(request);
            return Ok(ApiResponseBuilder.BuildResponse(message: "Update User Successfully", data: user));
        }

        [HttpDelete("{userId:guid}")]
        [Authorize]
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid id)
        {
            var user = await userService.GetUserByIdAsync(id);
            return Ok(ApiResponseBuilder.BuildResponse(data: mapper.Map<UserViewResponse>(user), message: "Get User Info Successfully"));
        }

        [HttpPatch("profile/avatar")]
        [ValidateFile]
        [Authorize(Roles = "ALOHA_ADMIN, ALOHA_USER")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUserAvatar(IFormFile file)
        {
            var userId = Guid.Parse(User.GetUserId());
            var user = await userService.UploadUserAvatar(userId, file);
            return Ok(ApiResponseBuilder.BuildResponse(message: "Update User Avatar Successfully", data: user));
        }
    }
}
