using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace VKB_WA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model.Username == "admin" && model.Password == "admin")
            {
                HttpContext.Response.Cookies.Append("auth", "true");
                return Ok();
            }
            return Unauthorized();
        }
    }

    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
