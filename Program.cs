using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VKB_WA.Services; // BotHostedService, CommandCacheService, CommandExecutor

var builder = WebApplication.CreateBuilder(args);

// ------------------ Сервисы ------------------

// HttpClient для внешних запросов
builder.Services.AddHttpClient();

// Фоновый сервис бота
builder.Services.AddSingleton<BotHostedService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BotHostedService>());

// Сервисы для админ-панели
builder.Services.AddSingleton<CommandCacheService>();
builder.Services.AddSingleton<CommandExecutor>();

// Контроллеры WebAPI
builder.Services.AddControllers();

// Swagger для тестирования API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------ Построение приложения ------------------

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers();

await app.RunAsync();
