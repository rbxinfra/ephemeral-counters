namespace Roblox.EphemeralCounters;

using System;
using System.Linq;
using System.Threading.Tasks;

using Prometheus;

using Redis;
using Platform.EphemeralCounters;

/// <summary>
/// A sequence.
/// </summary>
internal class Sequence : ISequence
{
    private static readonly Prometheus.Counter _TotalAdditionsCounter = Metrics.CreateCounter(
        "ephemeral_statistics_additions_total",
        "Total number of additions made to a specific sequence",
        "sequence_name"
    );
    private static readonly Prometheus.Counter _TotalFlushesCounter = Metrics.CreateCounter(
        "ephemeral_statistics_flushes_total",
        "Total number of flushes made to a specific sequence",
        "sequence_name"
    );

    private readonly string _SequenceName;
    private readonly IRedisClientProvider _Provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sequence"/> class.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <param name="provider">The <see cref="IRedisClientProvider"/> to use for Redis operations.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sequenceName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
    public Sequence(string sequenceName, IRedisClientProvider provider)
    {
        if (string.IsNullOrEmpty(sequenceName)) throw new ArgumentException("Sequence name cannot be null or empty.", nameof(sequenceName));

        _SequenceName = sequenceName;
        _Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <inheritdoc cref="ISequence.Add(double)"/>
    public void Add(double value)
    {
        _TotalAdditionsCounter.WithLabels(_SequenceName).Inc();


        _Provider.Client.Execute(_SequenceName, db => db.ListRightPush(_SequenceName, value));
    }

    /// <inheritdoc cref="ISequence.AddInBackground(double, Action{Exception})"/>
    public void AddInBackground(double value, Action<Exception> exceptionHandler = null)
        => Task.Run(() =>
        {
            try
            {
                Add(value);
            }
            catch (Exception e)
            {
                exceptionHandler?.Invoke(e);
            }
        });

    public ISequenceStatistics FlushStatistics()
    {
        _TotalFlushesCounter.WithLabels(_SequenceName).Inc();

        var stats = GetStatistics();

        _Provider.Client.Execute(_SequenceName, db => db.KeyDelete(_SequenceName));

        return stats;
    }

    public ISequenceStatistics GetStatistics()
    {
        var values = _Provider.Client.Execute(_SequenceName, db => db.ListRange(_SequenceName, 0, -1));
        var stats = new SequenceStatistics(values.Select(value => (double)value).ToArray());

        return stats;
    }
}
