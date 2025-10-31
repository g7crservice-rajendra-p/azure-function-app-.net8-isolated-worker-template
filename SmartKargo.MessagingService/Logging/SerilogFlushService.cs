using Microsoft.Extensions.Hosting;
using Serilog;

namespace SmartKargo.MessagingService.Logging
{
    /// <summary>
    /// A hosted service that ensures Serilog log buffers are properly flushed
    /// when the application is shutting down. This prevents log loss during abrupt termination.
    /// </summary>
    internal sealed class SerilogFlushService : IHostedService
    {
        private bool _isFlushed = false;

        /// <summary>
        /// No startup logic is required for this service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Called when the host is stopping.
        /// Ensures all Serilog sinks flush their buffered logs before shutdown.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!_isFlushed)
                {
                    _isFlushed = true;
                    //Log.Information("Flushing Serilog logs before shutdown...");
                    Log.CloseAndFlush();
                    //Console.WriteLine("✅ Serilog logs flushed successfully.");
                }
            }
            catch (Exception ex)
            {
                // In case Serilog has already been disposed, fallback to Console
                Console.Error.WriteLine($"[SerilogFlushService] Failed to flush logs: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
