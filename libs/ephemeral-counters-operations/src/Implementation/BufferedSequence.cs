namespace Roblox.EphemeralCounters;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using EventLog;
using Platform.EphemeralCounters;

/// <summary>
/// A buffered sequence.
/// </summary>
public class BufferedSequence
{
    private ConcurrentDictionary<string, double[]> _Current;
    private Timer _CommitTimer;
    private bool _Disposed;

    private readonly IEphemeralCounterFactory _Factory;
    private readonly ILogger _Logger;

    private static TimeSpan CommitInterval => TimeSpan.FromSeconds(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedSequence" /> class.
    /// </summary>
    /// <param name="factory">The <see cref="IEphemeralCounterFactory" />.</param>
    /// <param name="logger">An <see cref="ILogger" />.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="logger" /> is <see langword="null" />.
    /// </exception>
    public BufferedSequence(IEphemeralCounterFactory factory, ILogger logger)
    {
        _Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _Current = new ConcurrentDictionary<string, double[]>();

        InitializeTimer();
    }

    /// <summary>
    /// Add a value to the sequence.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key" /> is <see langword="null" />.</exception>
    public void Add(string key, double value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        _Current.AddOrUpdate(key, _ => [value], (_, v) =>
        {
            var newValue = new double[v.Length + 1];

            v.CopyTo(newValue, 0);
            newValue[v.Length] = value;

            return newValue;
        });
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the counter.
    /// </summary>
    /// <param name="disposing">Is disposing?</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_Disposed) return;

        if (disposing)
        {
            DoCommit();

            _CommitTimer?.Dispose();
        }

        _CommitTimer = null;
        _Disposed = true;
    }

    private void InitializeTimer()
    {
        _CommitTimer = new Timer(s => PauseTimerAndCommit(), null, CommitInterval, CommitInterval);
    }

    private void PauseTimerAndCommit()
    {
        _CommitTimer.Change(-1, -1);

        DoCommit();

        _CommitTimer.Change(CommitInterval, CommitInterval);
    }

    private void Commit(IEnumerable<KeyValuePair<string, double[]>> committableDictionary)
    {
        try
        {
            Parallel.ForEach(
                committableDictionary,
                kvp =>
                {
                    var sequence = _Factory.GetSequence(kvp.Key);

                    Array.ForEach(kvp.Value, sequence.Add);
                }
            );
        }
        catch (Exception ex)
        {
            _Logger.Error(ex);
        }
    }

    internal void DoCommit()
    {
        var newDictionary = new ConcurrentDictionary<string, double[]>();
        var toCommit = Interlocked.Exchange(ref _Current, newDictionary);

        Commit(toCommit);
    }

    /// <summary>
    /// The dtor.
    /// </summary>
    ~BufferedSequence()
    {
        try
        {
            DoCommit();
        }
        catch (Exception)
        {
        }

        Dispose(false);
    }
}
