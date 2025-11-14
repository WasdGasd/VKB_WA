using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace VKBot.Web.Services
{
    public class ErrorLogger
    {
        private readonly ILogger<ErrorLogger> _logger;
        private readonly string _logFilePath;

        public ErrorLogger(ILogger<ErrorLogger> logger)
        {
            _logger = logger;

            // Исправленный путь - создаем в текущей директории
            var currentDir = Directory.GetCurrentDirectory();
            _logFilePath = Path.Combine(currentDir, "logs", "errors.json");

            // Создаем директорию для логов если не существует
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir!);
                _logger.LogInformation("Created logs directory: {LogDir}", logDir);
            }
        }

        public async Task LogErrorAsync(Exception ex, string level = "ERROR", long? userId = null, object? additional = null)
        {
            try
            {
                var errorLog = new
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    UserId = userId,
                    Exception = new
                    {
                        Type = ex.GetType().Name,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    },
                    AdditionalData = additional
                };

                var logEntry = JsonSerializer.Serialize(errorLog, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Логируем в консоль
                _logger.LogError(ex, "Error occurred. Level: {Level}, UserId: {UserId}", level, userId);

                // Логируем в файл
                await File.AppendAllTextAsync(_logFilePath, logEntry + Environment.NewLine + Environment.NewLine);
            }
            catch (Exception logEx)
            {
                _logger.LogCritical(logEx, "Failed to write error log");
            }
        }
    }
}