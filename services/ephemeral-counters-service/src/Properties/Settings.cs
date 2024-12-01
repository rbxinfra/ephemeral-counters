namespace Roblox.EphemeralCounters.Service;

using EventLog;
using Configuration;
using Web.Framework.Services;

using static SettingsProvidersDefaults;

internal class Settings : BaseSettingsProvider<Settings>, IServiceSettings, ISettings
{
    /// <inheritdoc cref="IVaultProvider.Path"/>
    protected override string ChildPath => EphemeralCountersSettingsPath;

    /// <inheritdoc cref="IServiceSettings.ApiKey"/>
    public string ApiKey => GetOrDefault(nameof(ApiKey), string.Empty);

    /// <inheritdoc cref="IServiceSettings.LogLevel"/>
    public LogLevel LogLevel => GetOrDefault(nameof(LogLevel), LogLevel.Information);

    /// <inheritdoc cref="ISettings.CommitInterval"/>
    public TimeSpan CommitInterval => GetOrDefault(nameof(CommitInterval), TimeSpan.FromSeconds(30));


    /// <inheritdoc cref="IServiceSettings.VerboseErrorsEnabled"/>
    public bool VerboseErrorsEnabled => GetOrDefault(nameof(VerboseErrorsEnabled), false);

    /// <summary>
    /// Gets the consul service name for the ephemeral statistics.
    /// </summary>
    public string StatisticsRedisClusterConsulServiceName => GetOrDefault<string>(
        nameof(StatisticsRedisClusterConsulServiceName), 
        () => throw new InvalidOperationException("StatisticsRedisClusterConsulServiceName is not set.")
    );

    /// <summary>
    /// Gets the consul service name for the ephemeral counters.
    /// </summary>
    public string CountersRedisClusterConsulServiceName => GetOrDefault<string>(
        nameof(CountersRedisClusterConsulServiceName), 
        () => throw new InvalidOperationException("CountersRedisClusterConsulServiceName is not set.")
    );
}
