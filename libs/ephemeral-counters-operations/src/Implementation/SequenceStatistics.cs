namespace Roblox.EphemeralCounters;

using System;
using System.Linq;
using System.Collections.Generic;

using Platform.EphemeralCounters;

/// <summary>
/// Represents statistics for a sequence.
/// </summary>
internal class SequenceStatistics : ISequenceStatistics
{
    private readonly Dictionary<string, double> _Statistics;

    private static readonly Dictionary<string, double> _EmptyStatistics = new Dictionary<string, double>
    {
        { nameof(Maximum), 0 },
        { nameof(Minimum), 0 },
        { nameof(Sum), 0 },
        { nameof(Count), 0 },
        { nameof(Average), 0 },
        { nameof(StandardDeviation), 0 },
        { nameof(P01), 0 },
        { nameof(P05), 0 },
        { nameof(P25), 0 },
        { nameof(P50), 0 },
        { nameof(P75), 0 },
        { nameof(P95), 0 },
        { nameof(P99), 0 }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceStatistics"/> class.
    /// </summary>
    /// <param name="values">The values to calculate statistics for.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is null.</exception>
    public SequenceStatistics(double[] values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        
        if (values.Length == 0) 
            _Statistics = _EmptyStatistics;
        else
            _Statistics = new Dictionary<string, double>
            {
                { nameof(Maximum), values.Max() },
                { nameof(Minimum), values.Min() },
                { nameof(Sum), values.Sum() },
                { nameof(Count), values.Length },
                { nameof(Average), values.Average() },
                { nameof(StandardDeviation), GetStandardDeviation(values) },
                { nameof(P01), GetPercentile(values, 1) },
                { nameof(P05), GetPercentile(values, 5) },
                { nameof(P25), GetPercentile(values, 25) },
                { nameof(P50), GetPercentile(values, 50) },
                { nameof(P75), GetPercentile(values, 75) },
                { nameof(P95), GetPercentile(values, 95) },
                { nameof(P99), GetPercentile(values, 99) }
            };
    }

    private static double GetStandardDeviation(double[] values)
    {
        var average = values.Average();
        var sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();

        return Math.Sqrt(sumOfSquaresOfDifferences / values.Length);
    }

    private static double GetPercentile(double[] values, double percentile)
    {
        var sorted = values.OrderBy(x => x).ToArray();
        var index = (int)Math.Ceiling(percentile / 100 * sorted.Length) - 1;

        return sorted[index];
    }

    /// <summary>
    /// Gets the raw statistics.
    /// </summary>
    /// <value>The raw statistics.</value>
    public Dictionary<string, double> RawStatistics => _Statistics;

    /// <inheritdoc cref="ISequenceStatistics.Maximum"/>
    public double Maximum => GetStatisticByName(nameof(Maximum));

    /// <inheritdoc cref="ISequenceStatistics.Minimum"/>
    public double Minimum => GetStatisticByName(nameof(Minimum));

    /// <inheritdoc cref="ISequenceStatistics.Sum"/>
    public double Sum => GetStatisticByName(nameof(Sum));

    /// <inheritdoc cref="ISequenceStatistics.Count"/>
    public int Count => (int)GetStatisticByName(nameof(Count));

    /// <inheritdoc cref="ISequenceStatistics.Average"/>
    public double Average => GetStatisticByName(nameof(Average));

    /// <inheritdoc cref="ISequenceStatistics.StandardDeviation"/>
    public double StandardDeviation => GetStatisticByName(nameof(StandardDeviation));

    /// <inheritdoc cref="ISequenceStatistics.P01"/>
    public double P01 => GetStatisticByName(nameof(P01));

    /// <inheritdoc cref="ISequenceStatistics.P05"/>
    public double P05 => GetStatisticByName(nameof(P05));

    /// <inheritdoc cref="ISequenceStatistics.P25"/>
    public double P25 => GetStatisticByName(nameof(P25));

    /// <inheritdoc cref="ISequenceStatistics.P50"/>
    public double P50 => GetStatisticByName(nameof(P50));

    /// <inheritdoc cref="ISequenceStatistics.P75"/>
    public double P75 => GetStatisticByName(nameof(P75));

    /// <inheritdoc cref="ISequenceStatistics.P95"/>
    public double P95 => GetStatisticByName(nameof(P95));

    /// <inheritdoc cref="ISequenceStatistics.P99"/>
    public double P99 => GetStatisticByName(nameof(P99));

    /// <inheritdoc cref="ISequenceStatistics.GetStatisticByName(string)"/>
    public double GetStatisticByName(string statisticName) => _Statistics[statisticName];
}
