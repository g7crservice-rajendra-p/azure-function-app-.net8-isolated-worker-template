// ------------------------------------------------------------------------------------------------------
// File: ConfigCache.cs
// Project: SmartKargo.MessagingService
//
// PURPOSE:
// --------
// Shared in-memory cache of configuration key/value pairs used by all orchestrators and activities.
// Designed for Azure Functions .NET 8 Isolated Worker + Durable Functions.
//
// HOW IT WORKS:
// -------------
//  • When an orchestrator first runs, it calls `ConfigCache.EnsureInitializedFromOrchestratorAsync(ctx)`.
//  • The method checks whether the static dictionary is already populated on this host.
//    If not, it reads configuration data from the durable `ConfigEntity`.
//  • If the entity is empty, it signals `RefreshAsync` inside the entity to pull data from DB,
//    then reads again.
//  • The in-memory cache is then updated (only once per process, only when the orchestrator
//    is NOT replaying) to avoid redundant durable calls.
//
//  • Subsequent orchestrators and activities running on the same host read configuration
//    from memory using `ConfigCache.Get("Key")` — extremely fast.
//
// LIMITATIONS / NOTES:
// --------------------
//  • This cache is **per-host instance** (process-local). Each scaled-out Functions host keeps its own copy.
//    For strong global consistency, use an external cache (Redis, etc.).
//  • Do not call EnsureInitializedFromOrchestratorAsync() outside orchestrators — it uses orchestration APIs.
//  • To refresh config while the app is running, start an orchestrator that calls the same method again,
//    or have an admin function signal `ConfigEntity.RefreshAsync` and then call SetAllAsync().
//
// ------------------------------------------------------------------------------------------------------

using Microsoft.DurableTask;               // For TaskOrchestrationContext
using Microsoft.DurableTask.Entities;      // For EntityInstanceId
using SmartKargo.MessagingService.Functions.Entities;
using System.Collections.Concurrent;

namespace SmartKargo.MessagingService.Services
{
    /// <summary>
    /// Process-level static cache for configuration values loaded from the Durable ConfigEntity.
    /// </summary>
    public static class ConfigCache
    {
        // Underlying concurrent dictionary for thread-safe access.
        private static readonly ConcurrentDictionary<string, string> _dict =
            new(StringComparer.OrdinalIgnoreCase);

        // Indicates whether cache has been initialized on this host.
        private static volatile bool _initialized = false;

        // Semaphore ensures only one concurrent initialization/update attempt.
        private static readonly SemaphoreSlim _initLock = new(1, 1);

        // Timestamp of last successful refresh.
        public static DateTime? LastRefreshUtc { get; private set; }

        public static bool IsInitialized => _initialized;
        public static bool IsEmpty => _dict.IsEmpty;

        /// <summary>
        /// Replaces the entire cache contents atomically.
        /// Typically called internally from EnsureInitializedFromOrchestratorAsync().
        /// </summary>
        public static async Task SetAllAsync(IDictionary<string, string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _dict.Clear();
                foreach (var kvp in values)
                    _dict[kvp.Key] = kvp.Value ?? string.Empty;

                LastRefreshUtc = DateTime.UtcNow;
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Returns a defensive copy of current cache contents.
        /// </summary>
        public static IDictionary<string, string> Snapshot()
            => new Dictionary<string, string>(_dict, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Retrieves a single configuration value (null if not found).
        /// </summary>
        public static string Get(string key)
            => key != null && _dict.TryGetValue(key, out var v) ? v : "";

        /// <summary>
        /// Orchestration-aware initializer (deterministic).
        /// Always performs the durable entity calls via the orchestration context so that
        /// the same sequence of durable operations is recorded on the orchestration history.
        /// Only updates the process-level in-memory cache when the orchestrator is NOT replaying.
        /// </summary>
        public static async Task<IDictionary<string, string>> EnsureInitializedFromOrchestratorAsync(
            TaskOrchestrationContext ctx,
            string entityKey = "Config")
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            // NOTE: Do NOT short-circuit based on process-local _initialized here.
            // Doing so breaks determinism because different runs/replays can see different process state.
            var entityId = new EntityInstanceId(nameof(ConfigEntity), entityKey);

            // Always call GetAll via orchestration context (this schedules the entity read and is recorded).
            var config = await ctx.Entities.CallEntityAsync<IDictionary<string, string>>(entityId, "GetAll");

            // If empty, ask the entity to refresh from DB and read again (both calls recorded).
            if (config == null || config.Count == 0)
            {
                // Signal Refresh (durable call) then re-read.
                await ctx.Entities.CallEntityAsync(entityId, "RefreshAsync", new ConfigEntity.RefreshOptions { ForceRefresh = true });
                config = await ctx.Entities.CallEntityAsync<IDictionary<string, string>>(entityId, "GetAll");
            }

            // Now update the process-local cache as a side-effect, but ONLY when not replaying.
            // This guarantees no side-effect is applied during replay.
            if (!ctx.IsReplaying)
            {
                // Use the same semaphore / SetAllAsync to atomically replace the cache.
                if (config != null && config.Count > 0)
                {
                    await SetAllAsync(config).ConfigureAwait(false);
                }
                else
                {
                    // If the entity returned no data, set an empty map so future calls are cheap.
                    await SetAllAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);
                }
            }

            // Return the config map (may be empty).
            return config ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
