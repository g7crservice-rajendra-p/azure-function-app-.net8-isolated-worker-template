using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SmartKargo.MessagingService.Configurations;

namespace SmartKargo.MessagingService.Logging
{
    /// <summary>
    /// Provides base Serilog configuration used across all sinks (e.g., Console, File, Azure Blob, etc.).
    /// Sets minimum log levels, overrides noisy namespaces, and enriches log context with environment details.
    /// </summary>
    public abstract class BaseSerilogConfiguration : ISerilogConfiguration
    {
        /// <summary>
        /// Configures core Serilog settings, including minimum log levels and standard enrichments.
        /// Derived classes should extend this method to add custom sinks.
        /// </summary>
        /// <param name="loggerConfiguration">The Serilog logger configuration being built.</param>
        /// <param name="configuration">The application configuration (e.g., appsettings.json).</param>
        /// <param name="environment">The current host environment (Development, Production, etc.).</param>
        /// <returns>A configured <see cref="LoggerConfiguration"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the AppLogging section is missing or invalid.</exception>
        public virtual LoggerConfiguration Configure(
            LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            // Retrieve only the "AppLogging" section from appsettings.json
            var appLoggingSection = configuration.GetSection(nameof(AppConfig.AppLogging));
            var appLogging = appLoggingSection.Get<AppLoggingConfig>();

            if (appLogging == null)
            {
                throw new InvalidOperationException("Application logging configuration section is missing or malformed.");
            }

            // Determine minimum log level (default: Information)
            LogEventLevel minimumLogLevel = appLogging.GetMinimumLogLevel();

            // Build the base Serilog configuration
            return loggerConfiguration
                .MinimumLevel.Is(minimumLogLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

                // Common enrichments for all sinks
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", environment.ApplicationName)
                .Enrich.WithProperty("Environment", environment.EnvironmentName);
        }
    }
}
