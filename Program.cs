using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using VKBot.Web.Services;
using VKBot.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Configure settings
builder.Services.Configure<VkSettings>(builder.Configuration.GetSection("Vk"));

// HttpClient factory
builder.Services.AddHttpClient("vkclient");

// services
builder.Services.AddSingleton<ErrorLogger>();
builder.Services.AddHostedService<BotService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
