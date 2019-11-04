using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLWSecurityService.Services;
using System.Threading.Tasks;

namespace MLWSecurityService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        public class NewUserRequest
        {
            public string Username { get; set; }

            public string Password { get; set; }

            public string Email { get; set; }
        }

        [HttpPost]
        [Route("new")]
        public async Task<IActionResult> CreateUser([FromBody]NewUserRequest request)
        {
            // Check if the user already exists
            var user = await userService.Get(request.Username);

            if (user != null)
            {
                return new JsonResult(new { Message = "User already exists!" }) { StatusCode = 409 };
            }

            await userService.Create(request.Username, request.Password, request.Email);

            return new JsonResult(new { Message = "Success!" }) { StatusCode = 201 };
        }
    }
}
