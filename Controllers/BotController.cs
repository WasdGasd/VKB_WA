using Microsoft.AspNetCore.Mvc;
using VKB_WA.Services;

namespace VKB_WA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        private readonly BotHostedService _bot;
        private readonly CommandCacheService _cache;

        public BotController(BotHostedService bot, CommandCacheService cache)
        {
            _bot = bot;
            _cache = cache;
        }

        [HttpPost("start")] public IActionResult Start() { _bot.StartBot(); return Ok(); }
        [HttpPost("stop")] public IActionResult Stop() { _bot.StopBot(); return Ok(); }
        [HttpPost("reload")] public IActionResult Reload() { _bot.ReloadCommands(_cache); return Ok(); }
    }
}
