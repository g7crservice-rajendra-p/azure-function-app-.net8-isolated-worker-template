using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartKargo.MessagingService.Configurations;

namespace SmartKargo.MessagingService.Extensions
{
    public static class ApiConfigExtension
    {
        private const string AppSettingsFile = "appsettings.json";
        private const string AppSettingsEnvironmentFile = "appsettings.{0}.json";

        public static FunctionsApplicationBuilder AddApiConfiguration(this FunctionsApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // 1. Load config
            builder.Configuration
                   .SetBasePath(builder.Environment.ContentRootPath)
                   .AddJsonFile(AppSettingsFile, optional: false, reloadOnChange: true)
                   .AddJsonFile(
                        string.Format(AppSettingsEnvironmentFile, builder.Environment.EnvironmentName),
                        optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();

            // 2. Bind strongly-typed configuration
            var appConfig = builder.Configuration.Get<AppConfig>()
                            ?? throw new InvalidOperationException("Failed to bind AppConfig. Check appsettings.json");

            // 3. Register services
            builder.Services.AddSingleton(appConfig);
            builder.Services.Configure<AppConfig>(builder.Configuration);

            return builder;
        }
    }
}
