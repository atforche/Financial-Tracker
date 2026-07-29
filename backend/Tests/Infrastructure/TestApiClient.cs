using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Infrastructure;

/// <summary>
/// Sends test requests to the application API.
/// </summary>
internal sealed class TestApiClient(HttpClient client) : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Sends a GET request and returns its required response body.
    /// </summary>
    public async Task<TResponse> GetAsync<TResponse>(string uri)
    {
        TResponse? response = await client.GetFromJsonAsync<TResponse>(uri, JsonOptions);
        return response ?? throw new InvalidOperationException($"No response body was returned for {uri}.");
    }

    /// <summary>
    /// Sends a POST request and returns its required response body.
    /// </summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(uri, request, JsonOptions);
        _ = response.EnsureSuccessStatusCode();
        TResponse? body = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return body ?? throw new InvalidOperationException($"No response body was returned for {uri}.");
    }

    /// <inheritdoc/>
    public void Dispose() => client.Dispose();
}