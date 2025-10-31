namespace SmartKargo.MessagingService.Config
{
    public class AppOptions
    {
        public string MailServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseIMAP { get; set; }
    }
}
