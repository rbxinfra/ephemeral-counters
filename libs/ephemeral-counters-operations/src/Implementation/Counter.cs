namespace Roblox.EphemeralCounters;

using System;
using System.Threading.Tasks;

using Prometheus;

using Redis;

using ICounter = Platform.EphemeralCounters.ICounter;

/// <summary>
/// A counter.
/// </summary>
internal class Counter : ICounter
{
    private static readonly Prometheus.Counter _TotalDecrementCounter = Metrics.CreateCounter(
        "ephemeral_counters_deccrement_total",
        "The total number of deccrements to a specified counter",
        "counter_name"
    );
    private static readonly Prometheus.Counter _TotalIncrementCounter = Metrics.CreateCounter(
        "ephemeral_counters_increment_total",
        "The total number of increments to a specified counter",
        "counter_name"
    );
    private static readonly Prometheus.Counter _TotalDeletionsCounter = Metrics.CreateCounter(
        "ephemeral_counters_deletions_total",
        "The total number of deletions to a specified counter",
        "counter_name"
    );
    private static readonly Prometheus.Counter _TotalFlushesCounter = Metrics.CreateCounter(
        "ephemeral_counters_flushes_total",
        "The total number of flushes to a specified counter",
        "counter_name"
    );

    private readonly string _CounterKey;
    private readonly IRedisClientProvider _Provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class.
    /// </summary>
    /// <param name="counterKey">The key of the counter.</param>
    /// <param name="provider">The <see cref="IRedisClientProvider"/> to use for Redis operations.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="counterKey"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
    public Counter(string counterKey, IRedisClientProvider provider)
    {
        if (string.IsNullOrEmpty(counterKey)) throw new ArgumentException("Counter key cannot be null or empty.", nameof(counterKey));

        _CounterKey = counterKey;
        _Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <inheritdoc cref="ICounter.Decrement(int)"/>
    public void Decrement(int amount = 1)
    {
        _TotalDecrementCounter.WithLabels(_CounterKey).Inc();

        _Provider.Client.Execute(_CounterKey, db => db.StringDecrement(_CounterKey, amount));
    }

    /// <inheritdoc cref="ICounter.DecrementInBackground(int, Action{Exception})"/>
    public void DecrementInBackground(int value = 1, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                Decrement(value);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });

    /// <inheritdoc cref="ICounter.Delete"/>
    public void Delete()
    {
        _TotalDeletionsCounter.WithLabels(_CounterKey).Inc();

        _Provider.Client.Execute(_CounterKey, db => db.KeyDelete(_CounterKey));
    }

    /// <inheritdoc cref="ICounter.FlushCount"/>
    public long FlushCount() 
    {
        _TotalFlushesCounter.WithLabels(_CounterKey).Inc();

        var count = GetCount();

        Delete();

        return count;
    }

    /// <inheritdoc cref="ICounter.GetCount"/>
    public long GetCount() => (long)_Provider.Client.Execute(_CounterKey, db => db.StringGet(_CounterKey));

    /// <inheritdoc cref="ICounter.Increment(int)"/>
    public void Increment(int amount = 1)
    {
        _TotalIncrementCounter.WithLabels(_CounterKey).Inc();

        _Provider.Client.Execute(_CounterKey, db => db.StringIncrement(_CounterKey, amount));
    }

    /// <inheritdoc cref="ICounter.IncrementInBackground(int, Action{Exception})"/>
    public void IncrementInBackground(int value = 1, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                Increment(value);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });
}
