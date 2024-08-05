namespace Roblox.EphemeralCounters.Service;

using Redis;

/// <summary>
/// Provides Redis clients for the ephemeral counters service.
/// </summary>
public class RedisClientProviders
{
    /// <summary>
    /// Gets the Redis client provider for ephemeral counters.
    /// </summary>
    public IRedisClientProvider EphemeralCounters { get; set; }

    /// <summary>
    /// Gets the Redis client provider for ephemeral statistics.
    /// </summary>
    public IRedisClientProvider EphemeralStatistics { get; set; }
}
