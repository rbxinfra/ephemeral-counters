
namespace Roblox.EphemeralCounters;

using System;

using EventLog;
using Platform.EphemeralCounters;

/// <summary>
/// Implements a buffered ephemeral counter.
/// </summary>
public class BufferedEphemeralCounter : BufferedEphemeralCounterBase
{
    /// <inheritdoc cref="Roblox.Collections.BufferedCounterBase{TKey}.CommitInterval"/>
    protected override TimeSpan CommitInterval => TimeSpan.FromSeconds(30);

    /// <inheritdoc cref="BufferedEphemeralCounterBase.MaxDegreeOfParallelism"/>
    protected override int MaxDegreeOfParallelism => 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="BufferedEphemeralCounter"/> class.
    /// </summary>
    /// <param name="factory">The <see cref="IEphemeralCounterFactory"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public BufferedEphemeralCounter(IEphemeralCounterFactory factory, ILogger logger) 
        : base(factory, logger)
    {
    }
}
