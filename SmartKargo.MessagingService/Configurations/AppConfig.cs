using Serilog.Events;

namespace SmartKargo.MessagingService.Configurations
{
    public class DatabaseConfig
    {
        public string ReadWriteConnectionString { get; set; } = string.Empty;
        public string ReadOnlyConnectionString { get; set; } = string.Empty;
        public string ArchivalConnectionString { get; set; } = string.Empty;

    }

    public class AppLoggingConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string MinimumLogLevel { get; set; } = "Information";
        public int RetentionDays { get; set; } = 30;
        public LogEventLevel GetMinimumLogLevel() =>
           Enum.TryParse<LogEventLevel>(MinimumLogLevel, true, out var level)
               ? level
               : LogEventLevel.Information;
    }

    public class AppConfig
    {
        public DatabaseConfig Database { get; set; } = new();
        public AppLoggingConfig AppLogging { get; set; } = new();
        
    }
}
