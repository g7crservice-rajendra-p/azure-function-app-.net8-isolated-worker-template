using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace SmartKargo.MessagingService.Logging
{
    /// <summary>
    /// Configures Serilog to write logs to the console.
    /// Supports structured (JSON) or human-readable output depending on environment.
    /// </summary>
    public sealed class ConsoleSerilogConfiguration : BaseSerilogConfiguration
    {
        /// <summary>
        /// Configures the console sink with asynchronous, environment-aware formatting.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration being built.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The host environment (Development, Production, etc.).</param>
        /// <returns>A configured <see cref="LoggerConfiguration"/> instance.</returns>
        public override LoggerConfiguration Configure(
            LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Always start from the base configuration (minimum level, enrichments, etc.)
            var baseConfig = base.Configure(loggerConfiguration, configuration, environment);

            // Configure the console sink asynchronously to avoid blocking
            return base.Configure(loggerConfiguration, configuration, environment)
                .WriteTo.Async(a => a.Console(new CompactJsonFormatter()));
        }
    }
}
