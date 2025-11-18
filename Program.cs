using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VKB_WA.Services;
using VKBot.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<VkSettings>(builder.Configuration.GetSection("Vk"));

// ------------------ Сервисы ------------------

// CORS для frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// HttpClient для внешних запросов
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ErrorLogger>();

// Сервисы бота
builder.Services.AddSingleton<BotService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BotService>());

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
app.UseCors();
app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers();

await app.RunAsync();