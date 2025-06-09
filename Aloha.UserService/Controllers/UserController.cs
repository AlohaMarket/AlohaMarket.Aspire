using Aloha.UserService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.UserService.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await userService.GetUserByIdAsync(userId);
            return Ok(user);
        }

    }
}
