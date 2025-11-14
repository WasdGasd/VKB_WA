using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VKBot.Web.Models;

namespace VKBot.Web.Controllers
{
    [ApiController]
    [Route("api/vk/callback")]
    public class VkCallbackController : ControllerBase
    {
        private readonly VkSettings _vk;

        public VkCallbackController(IOptions<VkSettings> vkOptions)
        {
            _vk = vkOptions.Value;
        }

        [HttpPost]
        public IActionResult Post([FromBody] JsonElement body)
        {
            if (body.ValueKind != JsonValueKind.Object) return Ok("ok");
            if (body.TryGetProperty("type", out var t))
            {
                var type = t.GetString();
                if (type == "confirmation")
                {
                    return Ok(_vk.ConfirmationCode ?? "REPLACE_CONFIRMATION_CODE");
                }
            }
            return Ok("ok");
        }
    }
}
