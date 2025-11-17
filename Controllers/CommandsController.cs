using Microsoft.AspNetCore.Mvc;
using VKB_WA.Models;
using VKB_WA.Services;
using System.Collections.Generic;

namespace VKB_WA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private static List<BotCommand> _commands = new();

        [HttpGet]
        public IEnumerable<BotCommand> Get() => _commands;

        [HttpPost]
        public BotCommand Create([FromBody] BotCommand cmd)
        {
            cmd.Id = _commands.Count + 1;
            _commands.Add(cmd);
            return cmd;
        }

        [HttpPut("{id}")]
        public BotCommand Update(int id, [FromBody] BotCommand cmd)
        {
            var c = _commands.Find(x => x.Id == id);
            if (c != null)
            {
                c.Name = cmd.Name;
                c.ActionType = cmd.ActionType;
                c.ActionData = cmd.ActionData;
            }
            return c;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _commands.RemoveAll(x => x.Id == id);
            return Ok();
        }
    }
}
