using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace VKB_WA.Services
{
    public class BotHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BotHostedService> _logger;
        private bool _running;

        public BotHostedService(IServiceProvider services, ILogger<BotHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BotHostedService started.");
            _running = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_running)
                {
                    using var scope = _services.CreateScope();
                    var executor = scope.ServiceProvider.GetRequiredService<CommandExecutor>();
                    await executor.ProcessCommandsAsync();
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void StartBot() => _running = true;
        public void StopBot() => _running = false;
        public void ReloadCommands(CommandCacheService cache) => cache.Reload();
    }
}
