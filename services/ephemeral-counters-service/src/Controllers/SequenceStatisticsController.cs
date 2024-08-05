namespace Roblox.EphemeralCounters.Service.Controllers;

using System;

using Microsoft.AspNetCore.Mvc;

using EventLog;
using Platform.EphemeralCounters;

/// <summary>
/// Controller for accessing and modifying Api Clients.
/// </summary>
[Route("v1.0/[controller]")]
[ApiController]
public class SequenceStatisticsController : Controller
{
    private readonly ILogger _Logger;
    private readonly IEphemeralCounterFactory _EphemeralCounterFactory;

    private readonly BufferedSequence _BufferedSequences;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceStatisticsController"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="ephemeralCounterFactory">The <see cref="IEphemeralCounterFactory"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// - <paramref name="logger"/> cannot be null.
    /// - <paramref name="ephemeralCounterFactory"/> cannot be null.
    /// </exception>
    public SequenceStatisticsController(ILogger logger, IEphemeralCounterFactory ephemeralCounterFactory)
    {
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _EphemeralCounterFactory = ephemeralCounterFactory ?? throw new ArgumentNullException(nameof(ephemeralCounterFactory));

        _BufferedSequences = new BufferedSequence(_EphemeralCounterFactory, _Logger);
    }

    /// <summary>
    /// Add a value to a sequence.
    /// </summary>
    /// <param name="sequenceName">The sequence name.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// sequenceName is null or empty.
    /// </response>
    [HttpPost("AddToSequence")]
    [ProducesResponseType(400)]
    public ActionResult AddValueToSequence([FromQuery] string sequenceName, [FromQuery] double value, [FromQuery] bool isBufferedWrite = true)
    {
        if (string.IsNullOrEmpty(sequenceName)) return BadRequest("sequenceName is null or empty.");

        if (isBufferedWrite)
        {
            _BufferedSequences.Add(sequenceName, value);
        }
        else
        {
            var sequence = _EphemeralCounterFactory.GetSequence(sequenceName);

            sequence.Add(value);
        }

        return Ok();
    }

    /// <summary>
    /// Batch add values to a sequence.
    /// </summary>
    /// <remarks>
    /// <span>
    /// This request is limited to adding only 1 value per sequence.
    /// e.g. If you want to add 3 different values to the same sequence, you must make 3 separate requests.
    /// (v2 fixes this limitation, therefore this is obsolete but kept for backwards compatibility)
    /// <br />
    /// <br />
    /// </span>
    /// </remarks>
    /// <param name="entries">The sequence entries to add.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// entries is null or empty.
    /// </response>
    [HttpPost("BatchAddToSequences")]
    [ProducesResponseType(400)]
    [Obsolete("This method is obsolete and is kept for backwards compatibility.")]
    [ActionName("BatchAddValuesToSequences")]
    public ActionResult BatchAddValuesToSequences([FromBody] Dictionary<string, double> entries, [FromQuery] bool isBufferedWrite = true)
    {
        if (entries == null || entries.Count == 0) return BadRequest("entries is null or empty.");

        foreach (var entry in entries)
        {
            if (isBufferedWrite)
            {
                _BufferedSequences.Add(entry.Key, entry.Value);
            }
            else
            {
                var sequence = _EphemeralCounterFactory.GetSequence(entry.Key);

                sequence.Add(entry.Value);
            }
        }

        return Ok();
    }

    /// <summary>
    /// Batch add values to a sequence.
    /// </summary>
    /// <remarks>
    /// <span>
    /// This request is limited to adding only 1 value per sequence.
    /// e.g. If you want to add 3 different values to the same sequence, you must make 3 separate requests.
    /// (v2 fixes this limitation, therefore this is obsolete but kept for backwards compatibility)
    /// <br />
    /// <br />
    /// </span>
    /// </remarks>
    /// <param name="entries">The sequence entries to add.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// entries is null or empty.
    /// </response>
    [HttpPost("BatchAddToSequences")]
    [ProducesResponseType(400)]
    [Obsolete("This method is obsolete and is kept for backwards compatibility.")]
    [Consumes("application/x-www-form-urlencoded")]
    [ActionName("BatchAddValuesToSequences")]
    public ActionResult BatchAddValuesToSequencesFromForm([FromForm] Dictionary<string, double> entries, [FromQuery] bool isBufferedWrite = true)
        => BatchAddValuesToSequences(entries, isBufferedWrite);

    /// <summary>
    /// Batch add values to a sequence.
    /// </summary>
    /// <param name="sequenceEntries">The sequence entries to add.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// sequenceEntries is null or empty.
    /// </response>
    [HttpPost("BatchAddToSequencesV2")]
    [ProducesResponseType(400)]
    [ActionName("BatchAddValuesToSequences")]
    public ActionResult BatchAddValuesToSequences([FromBody] IEnumerable<KeyValuePair<string, double>> sequenceEntries, [FromQuery] bool isBufferedWrite = true)
    {
        if (sequenceEntries == null || !sequenceEntries.Any()) return BadRequest("sequenceEntries is null or empty.");

        foreach (var entry in sequenceEntries)
        {
            if (isBufferedWrite)
            {
                _BufferedSequences.Add(entry.Key, entry.Value);
            }
            else
            {
                var sequence = _EphemeralCounterFactory.GetSequence(entry.Key);

                sequence.Add(entry.Value);
            }
        }

        return Ok();
    }

    /// <summary>
    /// Batch add values to a sequence.
    /// </summary>
    /// <param name="sequenceEntries">The sequence entries to add.</param>
    /// <param name="isBufferedWrite">Whether or not to buffer the write.</param>
    /// <response code="400">
    /// sequenceEntries is null or empty.
    /// </response>
    [HttpPost("BatchAddToSequencesV2")]
    [ProducesResponseType(400)]
    [Consumes("application/x-www-form-urlencoded")]
    [ActionName("BatchAddValuesToSequences")]
    public ActionResult BatchAddValuesToSequencesFromForm([FromForm] IEnumerable<KeyValuePair<string, double>> sequenceEntries, [FromQuery] bool isBufferedWrite = true)
        => BatchAddValuesToSequences(sequenceEntries, isBufferedWrite);

    /// <summary>
    /// Gets the statistics for a sequence.
    /// </summary>
    /// <param name="sequenceName">The sequence name.</param>
    /// <returns>The sequence statistics.</returns>
    /// <response code="400">
    /// sequenceName is null or empty.
    /// </response>
    [HttpGet("GetSequenceStatistics")]
    [ProducesResponseType(200, Type = typeof(SequenceStatisticsPayload))]
    [ProducesResponseType(400)]
    public ActionResult<SequenceStatisticsPayload> GetSequenceStatistics([FromQuery] string sequenceName)
    {
        if (string.IsNullOrEmpty(sequenceName)) return BadRequest("sequenceName is null or empty.");

        var sequence = _EphemeralCounterFactory.GetSequence(sequenceName);
        var statistics = sequence.GetStatistics() as SequenceStatistics;

        return new SequenceStatisticsPayload(statistics.RawStatistics);
    }

    /// <summary>
    /// Flush the sequence.
    /// </summary>
    /// <param name="sequenceName">The sequence name.</param>
    /// <response code="400">
    /// sequenceName is null or empty.
    /// </response>
    [HttpPost("FlushSequenceStatistics")]
    [ProducesResponseType(200, Type = typeof(SequenceStatisticsPayload))]
    [ProducesResponseType(400)]
    public ActionResult<SequenceStatisticsPayload> FlushSequenceStatistics([FromQuery] string sequenceName)
    {
        if (string.IsNullOrEmpty(sequenceName)) return BadRequest("sequenceName is null or empty.");

        var sequence = _EphemeralCounterFactory.GetSequence(sequenceName);
        var statistics = sequence.FlushStatistics() as SequenceStatistics;

        return new SequenceStatisticsPayload(statistics.RawStatistics);
    }
}
