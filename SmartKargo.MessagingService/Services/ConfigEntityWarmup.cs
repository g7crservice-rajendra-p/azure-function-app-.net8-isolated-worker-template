using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SmartKargo.MessagingService.Functions.Entities;

namespace SmartKargo.MessagingService.Services
{
    /// <summary>
    /// Production-ready warmup helper for ConfigEntity.
    /// - Uses ConfigState.LastRefreshTime and ConfigCache.LastRefreshUtc to avoid unnecessary refreshes.
    /// - Prevents multiple concurrent refreshes using a process-wide semaphore.
    /// - Fast-path (cheap) when local cache is fresh.
    /// - Signals RefreshAsync and polls only when the entity is newer or cache is stale/empty.
    /// </summary>
    public static class ConfigEntityWarmup
    {
        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        /// <summary>
        /// Ensure the process-local ConfigCache is populated and reasonably fresh.
        /// Returns true if, after the call, the local cache contains one or more entries.
        /// </summary>
        public static async Task<bool> WarmupFromEntityAsync(
            DurableTaskClient durableClient,
            ILogger logger,
            CancellationToken cancellationToken,
            string entityKey = "config",
            TimeSpan? timeout = null,
            TimeSpan? pollDelay = null,
            TimeSpan? freshnessWindow = null) // how long local cache considered fresh
        {
            if (durableClient == null)
            {
                throw new ArgumentNullException(nameof(durableClient));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            timeout ??= TimeSpan.FromSeconds(30);
            pollDelay ??= TimeSpan.FromSeconds(2);
            freshnessWindow ??= TimeSpan.FromMinutes(1);

            var entityId = new EntityInstanceId(nameof(ConfigEntity), entityKey);
            ConfigState? state = null;

            try
            {
                // FAST PATH: if local cache exists and is recent enough, return immediately.
                if (ConfigCache.IsInitialized && !ConfigCache.IsEmpty && ConfigCache.LastRefreshUtc.HasValue)
                {
                    var age = DateTime.UtcNow - ConfigCache.LastRefreshUtc.Value;
                    if (age <= freshnessWindow.Value)
                    {
                        logger.LogDebug("ConfigCache fast-path: initialized and fresh (age={AgeSeconds:F1}s).", age.TotalSeconds);
                        return true; // cache has entries and is fresh
                    }

                    logger.LogDebug("ConfigCache considered stale (age={AgeSeconds:F1}s > freshnessWindow={WindowSeconds}s).",
                        age.TotalSeconds, freshnessWindow.Value.TotalSeconds);
                }

                // 1) Read entity state once (cheap). This gives us LastRefreshTime and any existing Config.
                var response = await durableClient.Entities.GetEntityAsync<ConfigState>(entityId).ConfigureAwait(false);
                state = (response != null && response.IncludesState) ? response.State : null;

                // If entity contains config, decide whether to use it based on timestamps.
                if (state?.Config != null && state.Config.Count > 0)
                {
                    // If we have a local timestamp, use it to decide
                    if (ConfigCache.LastRefreshUtc.HasValue)
                    {
                        // If entity is newer than local cache -> adopt entity contents
                        if (state.LastRefreshTime > ConfigCache.LastRefreshUtc.Value)
                        {
                            await ConfigCache.SetAllAsync(state.Config).ConfigureAwait(false);
                            logger.LogInformation("ConfigCache updated from entity (LastRefreshTime={EntityTime}, {Count} items).",
                                state.LastRefreshTime, state.Config.Count);
                            return true;
                        }
                        else
                        {
                            // Entity is not newer than local cache: keep local cache
                            if (ConfigCache.IsInitialized && !ConfigCache.IsEmpty)
                            {
                                logger.LogDebug("Entity not newer than local cache; skipping refresh.");
                                return true;
                            }

                            // Local cache is not populated for some reason — set from entity.
                            await ConfigCache.SetAllAsync(state.Config).ConfigureAwait(false);
                            logger.LogInformation("ConfigCache populated from entity (local cache was empty) with {Count} items.", state.Config.Count);
                            return true;
                        }
                    }
                    else
                    {
                        // No local timestamp => first-time population from entity
                        await ConfigCache.SetAllAsync(state.Config).ConfigureAwait(false);
                        logger.LogInformation("ConfigCache populated from entity (no local timestamp) with {Count} items.", state.Config.Count);
                        return true;
                    }
                }

                logger.LogInformation("ConfigEntity empty or not usable for immediate population. Proceeding to refresh flow.");

                // 2) Prevent concurrent refreshes — only one caller will attempt the refresh.
                await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    // Re-check local cache first — another caller may have refreshed while we waited.
                    if (ConfigCache.IsInitialized && !ConfigCache.IsEmpty && ConfigCache.LastRefreshUtc.HasValue)
                    {
                        var age = DateTime.UtcNow - ConfigCache.LastRefreshUtc.Value;
                        if (age <= freshnessWindow.Value)
                        {
                            logger.LogDebug("ConfigCache populated by another caller while waiting for lock; skipping refresh.");
                            return true;
                        }
                    }

                    // Now re-read entity state because local cache is still not usable.
                    response = await durableClient.Entities.GetEntityAsync<ConfigState>(entityId).ConfigureAwait(false);
                    state = (response != null && response.IncludesState) ? response.State : null;

                    if (state?.Config != null && state.Config.Count > 0)
                    {
                        // If entity has config now, set and return
                        await ConfigCache.SetAllAsync(state.Config).ConfigureAwait(false);
                        logger.LogInformation("ConfigCache populated from entity after re-check ({Count}).", state.Config.Count);
                        return true;
                    }

                    // Signal the entity to refresh from DB (non-blocking)
                    await durableClient.Entities.SignalEntityAsync(
                        entityId,
                        "RefreshAsync",
                        new ConfigEntity.RefreshOptions { ForceRefresh = true }).ConfigureAwait(false);

                    // Poll until entity contains config or timeout/cancellation occurs
                    var start = DateTime.UtcNow;
                    var deadline = start + timeout.Value;

                    while (DateTime.UtcNow < deadline)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(pollDelay.Value, cancellationToken).ConfigureAwait(false);

                        response = await durableClient.Entities.GetEntityAsync<ConfigState>(entityId).ConfigureAwait(false);
                        state = (response != null && response.IncludesState) ? response.State : null;

                        if (state?.Config != null && state.Config.Count > 0)
                        {
                            logger.LogInformation("ConfigEntity refresh completed in {Sec:F1}s.", (DateTime.UtcNow - start).TotalSeconds);
                            break;
                        }

                        logger.LogDebug("Waiting for ConfigEntity.RefreshAsync... elapsed {Sec:F1}s", (DateTime.UtcNow - start).TotalSeconds);
                    }
                }
                finally
                {
                    _refreshLock.Release();
                }

                // 3) Populate the local cache (may be empty)
                var cache = (state?.Config != null && state.Config.Count > 0)
                    ? state.Config
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                await ConfigCache.SetAllAsync(cache).ConfigureAwait(false);

                if (cache.Count > 0)
                {
                    logger.LogInformation("ConfigCache populated with {Count} entries (final).", cache.Count);
                    return true;
                }

                logger.LogWarning("ConfigEntity did not populate within {Timeout}s; using empty cache.", timeout.Value.TotalSeconds);
                return false;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Config warmup cancelled (host shutdown?). Using empty cache.");
                await ConfigCache.SetAllAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception during config warmup; falling back to empty cache.");
                await ConfigCache.SetAllAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);
                return false;
            }
        }
    }
}
