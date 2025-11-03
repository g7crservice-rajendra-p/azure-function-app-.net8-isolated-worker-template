using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SmartKargo.MessagingService.Functions.Entities;

public class ConfigReaderService
{
    private readonly DurableTaskClient _durableClient;
    private static readonly EntityInstanceId ConfigEntityId = new(nameof(ConfigEntity), "config");

    public ConfigReaderService(DurableTaskClient durableClient)
    {
        _durableClient = durableClient;
    }

    /// <summary>
    /// Returns a copy of all key/value pairs, or null if the entity does not exist.
    /// </summary>
    public async Task<IDictionary<string, string>?> GetAllAsync()
    {
        // Read the entity snapshot/state
        var entityResponse = await _durableClient.Entities.GetEntityAsync<ConfigState>(ConfigEntityId);
        // entityResponse may be null if entity doesn't exist or has no state
        return entityResponse?.State?.Config is null
            ? null
            : new Dictionary<string, string>(entityResponse.State.Config);
    }

    /// <summary>
    /// Get a single config value. Returns null if entity or key not present.
    /// </summary>
    public async Task<string?> GetAsync(string key)
    {
        var entityResponse = await _durableClient.Entities.GetEntityAsync<ConfigState>(ConfigEntityId);
        if (entityResponse?.State?.Config != null &&
            entityResponse.State.Config.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }
}
