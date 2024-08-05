namespace Roblox.EphemeralCounters;

using System;

/// <summary>
/// The settings for ephemeral counters.
/// </summary>
public interface ISettings
{
    /// <summary>
    /// Gets the commit interval for buffered ephemeral counters.
    /// </summary>
    TimeSpan CommitInterval { get; }
}
