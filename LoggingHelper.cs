using Serilog;
using Serilog.Core;
using Serilog.Events;

public static class LoggingHelper
{
    public static Logger ConfigureLogger()
    {
        // Создаем конфигурацию для логирования
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File("logs/info.log", restrictedToMinimumLevel: LogEventLevel.Information)  // Файл для записи простой информации
            .WriteTo.File("logs/error.log", restrictedToMinimumLevel: LogEventLevel.Error);  // Файл для записи ошибок

        // Создаем логгер
        var logger = loggerConfiguration.CreateLogger();

        // Устанавливаем глобальный логгер
        Log.Logger = logger;

        return logger;
    }
}
