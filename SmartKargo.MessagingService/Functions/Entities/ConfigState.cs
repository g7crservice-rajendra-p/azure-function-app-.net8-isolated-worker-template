namespace SmartKargo.MessagingService.Functions.Entities
{
    public class ConfigState
    {
        public IDictionary<string, string> Config { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public DateTime LastRefreshTime { get; set; } = DateTime.MinValue;
    }
}