// ------------------------------------------------------------------------------------------------------
// File: ConfigCache.cs
// Project: SmartKargo.MessagingService
//
// PURPOSE:
// --------
// Shared in-memory cache of configuration key/value pairs used by all orchestrators and activities.
// Designed for Azure Functions .NET 8 Isolated Worker + Durable Functions.
//
// LIMITATIONS / NOTES:
// --------------------
//  • This cache is **per-host instance** (process-local). Each scaled-out Functions host keeps its own copy.
//    For strong global consistency, use an external cache (Redis, etc.).
// ------------------------------------------------------------------------------------------------------

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
    }
}
