using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Entities;
using SmartKargo.MessagingService.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartKargo.MessagingService.Functions.Triggers
{
    /// <summary>
    /// Timer trigger that initializes ConfigCache from ConfigEntity.
    /// Waits synchronously until RefreshAsync completes.
    /// </summary>
    public class ConfigCacheWarmupTimeStarter
    {
        private readonly ILogger _logger;
        private readonly StartupReadiness _readiness;
        private const string EntityKey = "Config";

        public ConfigCacheWarmupTimeStarter(ILoggerFactory loggerFactory, StartupReadiness readiness)
        {
            _logger = loggerFactory.CreateLogger<ConfigCacheWarmupTimeStarter>();
            _readiness = readiness;
        }

        [Function("ConfigCacheWarmup")]
        public async Task Run(
            [TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo timerInfo,
            [DurableClient] DurableTaskClient durableClient)
        {
            _logger.LogInformation("ConfigCacheWarmup triggered at {Time}", DateTime.UtcNow);

            var entityId = new EntityInstanceId(nameof(ConfigEntity), EntityKey);

            try
            {
                // Step 1: Try reading current entity state
                var response = await durableClient.Entities.GetEntityAsync<ConfigState>(entityId);
                var state = (response != null && response.IncludesState) ? response.State : null;

                if (state != null && state.Config.Count > 0)
                {
                    await ConfigCache.SetAllAsync(state.Config);
                    _readiness.SignalReady();
                    _logger.LogInformation("ConfigCache loaded from existing ConfigEntity ({Count} items).", state.Config.Count);
                    return;
                }

                _logger.LogInformation("ConfigEntity empty. Triggering RefreshAsync...");

                // Step 2: Trigger refresh
                await durableClient.Entities.SignalEntityAsync(
                    entityId,
                    "RefreshAsync",
                    new ConfigEntity.RefreshOptions { ForceRefresh = true });

                // Step 3: Wait synchronously (poll) until entity refresh completes
                var startTime = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(30); // configurable
                var delay = TimeSpan.FromSeconds(2);

                while (DateTime.UtcNow - startTime < timeout)
                {
                    await Task.Delay(delay);

                    response = await durableClient.Entities.GetEntityAsync<ConfigState>(entityId);
                    state = (response != null && response.IncludesState) ? response.State : null;

                    if (state != null && state.Config.Count > 0)
                    {
                        _logger.LogInformation("Entity refresh complete after {Seconds:F1}s", (DateTime.UtcNow - startTime).TotalSeconds);
                        break;
                    }

                    _logger.LogInformation("Waiting for ConfigEntity to finish RefreshAsync...");
                }

                // Step 4: Update cache (even if empty fallback)
                var cache = (state?.Config != null && state.Config.Count > 0)
                    ? state.Config
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                await ConfigCache.SetAllAsync(cache);
                _readiness.SignalReady();

                if (cache.Count > 0)
                {
                    _logger.LogInformation("ConfigCache populated with {Count} entries.", cache.Count);
                }
                else
                {
                    _logger.LogWarning("ConfigEntity did not populate within {Timeout}s; using empty cache.", timeout.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                await ConfigCache.SetAllAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
                _readiness.SignalReady();
                _logger.LogError(ex, "Error during ConfigCache warmup. Using empty cache as fallback.");
            }
        }
    }
}
