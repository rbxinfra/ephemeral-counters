namespace Roblox.EphemeralCounters.Service;

using System;
using System.Linq;

using Roblox.Web.Framework.Services.Http;

using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

/// <summary>
/// An <see cref="IApiKeyParser"/> that parses the ApiKey from a request header.
/// </summary>
/// <remarks>Used for backwards compatibility with the old way of passing the ApiKey.</remarks>
/// <seealso cref="IApiKeyParser"/>
public class ApiKeyParser : IApiKeyParser
{
    /// <summary>
    /// The name of the api key header
    /// </summary>
    public const string ApiKeyHeaderName = "Roblox-Api-Key";

    private const string _ApiKeyQueryName = "apiKey";

    /// <inheritdoc cref="IApiKeyParser.TryParseApiKey"/>
    public bool TryParseApiKey(HttpRequest request, out Guid apiKey)
    {
        apiKey = default;

        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var rawApiKey)
            && Guid.TryParse(rawApiKey, out apiKey)) return true;

        return request.Query.TryGetValue(_ApiKeyQueryName, out var values)
            && Guid.TryParse(values.FirstOrDefault(), out apiKey);
    }
}
