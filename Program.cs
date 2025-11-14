using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VKBot.Web.Services;
using VKBot.Web.Models;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.Configure<VkSettings>(context.Configuration.GetSection("Vk"));
        services.AddSingleton<ErrorLogger>();
        services.AddHostedService<BotService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.SetMinimumLevel(LogLevel.Information);
    });

var host = builder.Build();
await host.RunAsync();