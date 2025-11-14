using Microsoft.AspNetCore.Mvc;
using VKBot.Web.Services;

namespace VKBot.Web.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ErrorLogger _logger;
        public AdminController(ErrorLogger logger) => _logger = logger;

        [HttpGet("errors")]
        public async Task<IActionResult> GetRecentErrors(int limit = 20)
        {
            var items = await _logger.GetRecentErrorsAsync(limit);
            return Ok(items);
        }
    }
}
