namespace Roblox.EphemeralCounters;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Redis;
using Platform.EphemeralCounters;

/// <summary>
/// Implements <see cref="IEphemeralCounterFactory"/>.
/// </summary>
public class EphemeralCounterFactory : IEphemeralCounterFactory
{
    private readonly IRedisClientProvider _ProviderForCounters;
    private readonly IRedisClientProvider _ProviderForSequences;

    /// <summary>
    /// Initializes a new instance of the <see cref="EphemeralCounterFactory"/> class.
    /// </summary>
    /// <param name="providerForCounters">The <see cref="IRedisClientProvider"/> to use for counter operations.</param>
    /// <param name="providerForSequences">The <see cref="IRedisClientProvider"/> to use for sequence operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="providerForCounters"/> or <paramref name="providerForSequences"/> is <see langword="null"/>.</exception>
    public EphemeralCounterFactory(IRedisClientProvider providerForCounters, IRedisClientProvider providerForSequences)
    {
        _ProviderForCounters = providerForCounters ?? throw new ArgumentNullException(nameof(providerForCounters));
        _ProviderForSequences = providerForSequences ?? throw new ArgumentNullException(nameof(providerForSequences));
    }

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchAddToSequences(IDictionary{string, double})"/>
    public void BatchAddToSequences(IDictionary<string, double> entries)
    {
        foreach (var entry in entries)
        {
            var sequence = GetSequence(entry.Key);

            sequence.Add(entry.Value);
        }
    }

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchAddToSequencesInBackground(IDictionary{string, double}, Action{Exception})"/>
    public void BatchAddToSequencesInBackground(IDictionary<string, double> entries, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                BatchAddToSequences(entries);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchIncrementCounters(IDictionary{string, long})"/>
    public void BatchIncrementCounters(IDictionary<string, long> entries)
    {
        foreach (var entry in entries)
        {
            var counter = GetCounter(entry.Key);

            counter.Increment((int)entry.Value);
        }
    }

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchIncrementCounters(IEnumerable{string})"/>
    public void BatchIncrementCounters(IEnumerable<string> counterNames)
    {
        foreach (var counterName in counterNames)
        {
            var counter = GetCounter(counterName);

            counter.Increment();
        }
    }

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchIncrementCountersInBackground(IDictionary{string, long}, Action{Exception})"/>
    public void BatchIncrementCountersInBackground(IDictionary<string, long> entries, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                BatchIncrementCounters(entries);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });

    /// <inheritdoc cref="IEphemeralCounterFactory.BatchIncrementCountersInBackground(IEnumerable{string}, Action{Exception})"/>
    public void BatchIncrementCountersInBackground(IEnumerable<string> counterNames, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                BatchIncrementCounters(counterNames);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });

    /// <inheritdoc cref="IEphemeralCounterFactory.GetCounter(string)"/>
    public ICounter GetCounter(string counterName) => new Counter(counterName, _ProviderForCounters);

    /// <inheritdoc cref="IEphemeralCounterFactory.GetSequence(string)"/>
    public ISequence GetSequence(string sequenceName) => new Sequence(sequenceName, _ProviderForSequences);
}
