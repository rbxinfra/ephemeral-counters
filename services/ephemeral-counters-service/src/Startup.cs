namespace Roblox.EphemeralCounters.Service;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Roblox.Web.Framework.Services;
using Roblox.Web.Framework.Services.Http;

using Roblox.Redis;
using Roblox.EventLog;
using Roblox.Configuration;
using Roblox.ServiceDiscovery;
using Roblox.Platform.EphemeralCounters;

/// <summary>
/// Startup class for ephemeral-counters-service.
/// </summary>
public class Startup : HttpStartupBase
{
    /// <inheritdoc cref="StartupBase.Settings"/>
    protected override IServiceSettings Settings => EphemeralCounters.Service.Settings.Singleton;

    /// <inheritdoc cref="StartupBase.ConfigureServices(IServiceCollection)"/>
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
            options.SuppressConsumesConstraintForFormFileParameters = true;
        });

        services.AddSingleton(provider =>
        {
            var logger = provider.GetRequiredService<ILogger>();

            var providers = new RedisClientProviders();
            var consulProvider = new LocalConsulClientProvider();

            var countersServiceResolver = new ConsulHttpServiceResolver(
                logger,
                consulProvider,
                EphemeralCounters.Service.Settings.Singleton.ToSingleSetting(s => s.CountersRedisClusterConsulServiceName),
                EnvironmentNameProvider.EnvironmentName
            );

            var statisticsServiceResolver = new ConsulHttpServiceResolver(
                logger,
                consulProvider,
                EphemeralCounters.Service.Settings.Singleton.ToSingleSetting(s => s.StatisticsRedisClusterConsulServiceName),
                EnvironmentNameProvider.EnvironmentName
            );

            providers.EphemeralCounters = new DiscoveringRedisClientProvider(
                logger,
                countersServiceResolver
            );
            providers.EphemeralStatistics = new DiscoveringRedisClientProvider(
                logger,
                statisticsServiceResolver
            );

            return providers;
        });

        services.AddSingleton<IEphemeralCounterFactory>(provider =>
        {
            var redisClientProviders = provider.GetRequiredService<RedisClientProviders>();

            return new EphemeralCounters.EphemeralCounterFactory(redisClientProviders.EphemeralCounters, redisClientProviders.EphemeralStatistics);
        });
        services.AddSingleton<EphemeralCounters.ISettings>(EphemeralCounters.Service.Settings.Singleton);
    }

    /// <inheritdoc cref="HttpStartupBase.ConfigureApiKeyParser(IServiceCollection)"/>
    protected override IApiKeyParser ConfigureApiKeyParser(IServiceCollection services) => new ApiKeyParser();
}
