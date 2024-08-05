using Roblox.Web.Framework.Services.Http;

namespace Roblox.EphemeralCounters.Service;

/// <summary>
/// The data to post to increment counters.
/// </summary>
public class CounterPayload : Payload<long>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CounterPayload"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    public CounterPayload(long data) 
        : base(data)
    {
    }
}
