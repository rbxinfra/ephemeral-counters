namespace Roblox.EphemeralCounters.Service;

/// <summary>
/// The data to post to increment counters.
/// </summary>
public class IncrementCountersPostData
{
    /// <summary>
    /// Gets or sets the counter names csv.
    /// </summary>
    public string CounterNamesCsv { get; set; }
}
