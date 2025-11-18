using Microsoft.AspNetCore.Mvc;
using VKB_WA.Models;
using System.Diagnostics;

namespace VKB_WA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private static readonly Random _random = new Random();

        [HttpGet("commands")]
        public IActionResult GetCommandStats()
        {
            var stats = new CommandStats
            {
                TotalExecuted = 5678,
                DailyUsage = GenerateDailyUsage(),
                PopularCommands = GeneratePopularCommands()
            };

            return Ok(stats);
        }

        [HttpGet("users")]
        public IActionResult GetUserStats()
        {
            var stats = new UserStats
            {
                TotalUsers = 1250,
                ActiveToday = 67,
                HourlyActivity = GenerateHourlyActivity()
            };

            return Ok(stats);
        }

        [HttpGet("system")]
        public IActionResult GetSystemStats()
        {
            var process = Process.GetCurrentProcess();
            var stats = new SystemStats
            {
                ResponseTime = "124ms",
                MemoryUsage = $"{Math.Round((double)process.WorkingSet64 / 1024 / 1024, 1)} MB",
                CpuLoad = "23%",
                Uptime = "24h 30m"
            };

            return Ok(stats);
        }

        private List<CommandUsage> GenerateDailyUsage()
        {
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            var usage = new List<CommandUsage>();

            foreach (var day in days)
            {
                usage.Add(new CommandUsage
                {
                    Date = day,
                    Count = _random.Next(50, 150)
                });
            }

            return usage;
        }

        private List<PopularCommand> GeneratePopularCommands()
        {
            return new List<PopularCommand>
            {
                new PopularCommand { Name = "start", UsageCount = 1567 },
                new PopularCommand { Name = "билеты", UsageCount = 1420 },
                new PopularCommand { Name = "загруженность", UsageCount = 980 },
                new PopularCommand { Name = "информация", UsageCount = 850 }
            };
        }

        private List<UserActivity> GenerateHourlyActivity()
        {
            var hours = new[] { "00:00", "04:00", "08:00", "12:00", "16:00", "20:00" };
            var activity = new List<UserActivity>();

            foreach (var hour in hours)
            {
                activity.Add(new UserActivity
                {
                    Time = hour,
                    Count = _random.Next(5, 25)
                });
            }

            return activity;
        }
    }
}