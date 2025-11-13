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

    public class AuthenticationConfig
    {
        public string AccessTokenUrl { get; set; } = string.Empty;
        public string BasicAuthenticationHeader { get; set; } = string.Empty;

    }

    public class SmsConfig
    {
        public string SMSUn { get; set; } = string.Empty;
        public string SMSPass { get; set; } = string.Empty;
        public string SendSMSUrl { get; set; } = string.Empty;
        public bool IsSMSNewApi { get; set; } = false;
    }

    public class PollingConfig
    {
        public int SleepSeconds { get; set; } = 0;
    }

    public class AlertConfig
    {
        public string DataDumpAlertEmailID { get; set; } = string.Empty;
    }

    public class MiscellaneousConfig
    {
        public string UTCORLOCALTIME { get; set; } = string.Empty;
        public string DownLoadFilePath { get; set; } = string.Empty;
        public string XMLFilePath { get; set; } = string.Empty;
        
    }

    //SFTP config
    public class SftpConfig
    {
        public string HostName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SshHostKeyFingerprint { get; set; } = string.Empty;
    }

    public class AppConfig
    {
        public DatabaseConfig Database { get; set; } = new();
        public AppLoggingConfig AppLogging { get; set; } = new();
        public AuthenticationConfig Authentication { get; set; } = new();
        public SmsConfig Sms { get; set; } = new();
        public PollingConfig Polling { get; set; } = new();
        public AlertConfig Alert { get; set; } = new();
        public MiscellaneousConfig Miscellaneous { get; set; } = new();
        public SftpConfig Sftp { get; set; } = new();


    }
}
