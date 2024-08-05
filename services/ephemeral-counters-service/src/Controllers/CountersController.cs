namespace Roblox.EphemeralCounters.Service.Controllers;

using System;

using Microsoft.AspNetCore.Mvc;

using EventLog;
using Platform.EphemeralCounters;

/// <summary>
/// Controller for accessing and modifying Api Clients.
/// </summary>
[Route("v1.1/[controller]")]
[ApiController]
public class CountersController : Controller
{
    private readonly ILogger _Logger;
    private readonly IEphemeralCounterFactory _EphemeralCounterFactory;

    private readonly BufferedEphemeralCounter _BufferedEphemeralCounter;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CountersController"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="ephemeralCounterFactory">The <see cref="IEphemeralCounterFactory"/>.</param>
    /// <param name="redisClientProviders">The <see cref="RedisClientProviders"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="logger"/> cannot be null.
    /// - <paramref name="ephemeralCounterFactory"/> cannot be null.
    /// - <paramref name="redisClientProviders"/> cannot be null.
    /// </exception>
    public CountersController(ILogger logger, IEphemeralCounterFactory ephemeralCounterFactory, RedisClientProviders redisClientProviders)
    {
        if (redisClientProviders == null) throw new ArgumentNullException(nameof(redisClientProviders));

        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _EphemeralCounterFactory = ephemeralCounterFactory ?? throw new ArgumentNullException(nameof(ephemeralCounterFactory));

        _BufferedEphemeralCounter = new BufferedEphemeralCounter(_EphemeralCounterFactory, _Logger);
    }

    /// <summary>
    /// Decrement the specified counter.
    /// </summary>
    /// <param name="counterName">The counter name.</param>
    /// <param name="amount">The amount to decrement by.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// counterName is null or empty.
    /// </response>
    [HttpPost("Decrement")]
    [ProducesResponseType(400)]
    public ActionResult DecrementCounter([FromQuery] string counterName, [FromQuery] int amount = 1, [FromQuery] bool isBufferedWrite = true)
    {
        if (string.IsNullOrEmpty(counterName)) return BadRequest("counterName is null or empty.");

        if (isBufferedWrite) {
            _BufferedEphemeralCounter.Increment(counterName, -amount);

            return Ok();
        }

        var counter = _EphemeralCounterFactory.GetCounter(counterName);

        counter.Decrement(amount);

        return Ok();
    }

    /// <summary>
    /// Increment the specified counter.
    /// </summary>
    /// <param name="counterName">The counter name.</param>
    /// <param name="amount">The amount to increment by.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// counterName is null or empty.
    /// </response>
    [HttpPost("Increment")]
    [ProducesResponseType(400)]
    public ActionResult IncrementCounter([FromQuery] string counterName, [FromQuery] int amount = 1, [FromQuery] bool isBufferedWrite = true)
    {
        if (string.IsNullOrEmpty(counterName)) return BadRequest("counterName is null or empty.");

        if (isBufferedWrite) {
            _BufferedEphemeralCounter.Increment(counterName, amount);

            return Ok();
        }

        var counter = _EphemeralCounterFactory.GetCounter(counterName);

        counter.Increment(amount);

        return Ok();
    }

    /// <summary>
    /// Delete the specified counter.
    /// </summary>
    /// <param name="counterName">The counter name.</param>
    /// <response code="400">
    /// counterName is null or empty.
    /// </response>
    [HttpPost("Delete")]
    [ProducesResponseType(400)]
    public ActionResult DeleteCounter([FromQuery] string counterName)
    {
        if (string.IsNullOrEmpty(counterName)) return BadRequest("counterName is null or empty.");

        var counter = _EphemeralCounterFactory.GetCounter(counterName);

        counter.Delete();

        return Ok();
    }

    /// <summary>
    /// Flush the specified counter.
    /// </summary>
    /// <param name="counterName">The counter name.</param>
    /// <returns>The result of the flush operation.</returns>
    /// <response code="400">
    /// counterName is null or empty.
    /// </response>
    [HttpPost("Flush")]
    [ProducesResponseType(200, Type = typeof(CounterPayload))]
    [ProducesResponseType(400)]
    public ActionResult<CounterPayload> FlushCounter([FromQuery] string counterName)
    {
        if (string.IsNullOrEmpty(counterName)) return BadRequest("counterName is null or empty.");

        var counter = _EphemeralCounterFactory.GetCounter(counterName);

        return new CounterPayload(counter.FlushCount());
    }

    /// <summary>
    /// Batch flush the specified counters.
    /// </summary>
    /// <param name="counterNames">The counter names.</param>
    /// <returns>A key-value pair of counter names and their respective flush results.</returns>
    /// <response code="400">
    /// counterNames is null or empty.
    /// </response>
    [HttpPost("BatchFlush")]
    [ProducesResponseType(200, Type = typeof(BatchFlushCountersPayload))]
    [ProducesResponseType(400)]
    [ActionName("BatchFlushCounters")]
    public ActionResult<BatchFlushCountersPayload> BatchFlushCounters([FromBody] IEnumerable<string> counterNames)
    {
        if (counterNames == null || !counterNames.Any()) return BadRequest("counterNames is null or empty.");

        var results = new Dictionary<string, long>();

        foreach (var counterName in counterNames)
        {
            var counter = _EphemeralCounterFactory.GetCounter(counterName);

            results.Add(counterName, counter.FlushCount());
        }

        return new BatchFlushCountersPayload(results);
    }

    /// <summary>
    /// Batch flush the specified counters.
    /// </summary>
    /// <param name="counterNames">The counter names.</param>
    /// <returns>A key-value pair of counter names and their respective flush results.</returns>
    /// <response code="400">
    /// counterNames is null or empty.
    /// </response>
    [HttpPost("BatchFlush")]
    [ProducesResponseType(200, Type = typeof(BatchFlushCountersPayload))]
    [ProducesResponseType(400)]
    [Consumes("application/x-www-form-urlencoded")]
    [ActionName("BatchFlushCounters")]
    public ActionResult<BatchFlushCountersPayload> BatchFlushCountersFromForm([FromForm] IEnumerable<string> counterNames)
        => BatchFlushCounters(counterNames);

    /// <summary>
    /// Get the value of the specified counter.
    /// </summary>
    /// <param name="counterName">The counter name.</param>
    /// <returns>The value of the counter.</returns>
    /// <response code="400">
    /// counterName is null or empty.
    /// </response>
    [HttpGet("Get")]
    [ProducesResponseType(200, Type = typeof(CounterPayload))]
    [ProducesResponseType(400)]
    public ActionResult<CounterPayload> GetCounter([FromQuery] string counterName)
    {
        if (string.IsNullOrEmpty(counterName)) return BadRequest("counterName is null or empty.");

        var counter = _EphemeralCounterFactory.GetCounter(counterName);

        return new CounterPayload(counter.GetCount());
    }

    /// <summary>
    /// Multi increment the specified counters.
    /// </summary>
    /// <param name="postData">The counter names to increment.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// postData is null or empty.
    /// </response>
    [HttpPost("MultiIncrement")]
    [ProducesResponseType(400)]
    [ActionName("IncrementCounters")]
    public ActionResult IncrementCounters([FromBody] IncrementCountersPostData postData, [FromQuery] bool isBufferedWrite = true)
    {
        if (postData == null || string.IsNullOrEmpty(postData.CounterNamesCsv)) return BadRequest("postData is null or empty.");

        var counterNames = postData.CounterNamesCsv.Split(',');

        if (isBufferedWrite)
        {
            foreach (var counterName in counterNames)
                _BufferedEphemeralCounter.Increment(counterName);

            return Ok();
        }

        foreach (var counterName in counterNames)
        {
            var counter = _EphemeralCounterFactory.GetCounter(counterName);

            counter.Increment();
        }

        return Ok();
    }

    /// <summary>
    /// Multi increment the specified counters.
    /// </summary>
    /// <param name="postData">The counter names to increment.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// postData is null or empty.
    /// </response>
    [HttpPost("MultiIncrement")]
    [ProducesResponseType(400)]
    [Consumes("application/x-www-form-urlencoded")]
    [ActionName("IncrementCounters")]
    public ActionResult IncrementCountersFromForm([FromForm] IncrementCountersPostData postData, [FromQuery] bool isBufferedWrite = true)
        => IncrementCounters(postData, isBufferedWrite);

    /// <summary>
    /// Batch increment the specified counters.
    /// </summary>
    /// <param name="entries">The counter names and their respective amounts to increment by.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// entries is null or empty.
    /// </response>
    [HttpPost("BatchIncrement")]
    [ProducesResponseType(400)]
    [ActionName("BatchIncrementCounters")]
    public ActionResult BatchIncrementCounters([FromBody] Dictionary<string, long> entries, [FromQuery] bool isBufferedWrite = true)
    {
        if (entries == null || entries.Count == 0) return BadRequest("entries is null or empty.");

        if (isBufferedWrite)
        {
            foreach (var entry in entries)
                _BufferedEphemeralCounter.Increment(entry.Key, entry.Value);

            return Ok();
        }

        foreach (var entry in entries)
        {
            var counter = _EphemeralCounterFactory.GetCounter(entry.Key);

            counter.Increment((int)entry.Value);
        }

        return Ok();
    }

    /// <summary>
    /// Batch increment the specified counters.
    /// </summary>
    /// <param name="entries">The counter names and their respective amounts to increment by.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// entries is null or empty.
    /// </response>
    [HttpPost("BatchIncrement")]
    [ProducesResponseType(400)]
    [Consumes("application/x-www-form-urlencoded")]
    [ActionName("BatchIncrementCounters")]
    public ActionResult BatchIncrementCountersFromForm([FromForm] Dictionary<string, long> entries, [FromQuery] bool isBufferedWrite = true)
        => BatchIncrementCounters(entries, isBufferedWrite);
}
