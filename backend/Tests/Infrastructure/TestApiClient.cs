using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.Infrastructure;

/// <summary>
/// Sends test requests to the application API.
/// </summary>
internal sealed class TestApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private const string TestUserHeader = "X-Test-User";

    private readonly HttpClient _client;

    /// <summary>
    /// Creates a client authenticated as the default test user.
    /// </summary>
    public TestApiClient(HttpClient client)
    {
        _client = client;
        client.DefaultRequestHeaders.Add(TestUserHeader, "test-user");
    }

    /// <summary>
    /// Sends a GET request and returns its required response body.
    /// </summary>
    public async Task<TResponse> GetAsync<TResponse>(string uri)
    {
        TResponse? response = await _client.GetFromJsonAsync<TResponse>(uri, JsonOptions);
        return response ?? throw new InvalidOperationException($"No response body was returned for {uri}.");
    }

    /// <summary>
    /// Sends a GET request and returns the response for status-code assertions.
    /// </summary>
    public Task<HttpResponseMessage> GetResponseAsync(string uri) => _client.GetAsync(new Uri(uri, UriKind.Relative));

    /// <summary>
    /// Sends a POST request and returns its required response body.
    /// </summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(uri, request, JsonOptions);
        _ = response.EnsureSuccessStatusCode();
        TResponse? body = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return body ?? throw new InvalidOperationException($"No response body was returned for {uri}.");
    }

    /// <summary>
    /// Sends a POST request that does not require a response body.
    /// </summary>
    public async Task PostAsync<TRequest>(string uri, TRequest request)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(uri, request, JsonOptions);
        _ = response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sends a POST request and returns the response for status-code assertions.
    /// </summary>
    public Task<HttpResponseMessage> PostResponseAsync<TRequest>(string uri, TRequest request) =>
        _client.PostAsJsonAsync(uri, request, JsonOptions);

    /// <summary>
    /// Sends a POST request with no request body.
    /// </summary>
    public async Task PostAsync(string uri)
    {
        using HttpResponseMessage response = await _client.PostAsync(new Uri(uri, UriKind.Relative), null);
        _ = response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    public async Task DeleteAsync(string uri)
    {
        using HttpResponseMessage response = await _client.DeleteAsync(new Uri(uri, UriKind.Relative));
        _ = response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sends a DELETE request and returns the response for status-code assertions.
    /// </summary>
    public Task<HttpResponseMessage> DeleteResponseAsync(string uri) => _client.DeleteAsync(new Uri(uri, UriKind.Relative));

    /// <inheritdoc/>
    public void Dispose() => _client.Dispose();
}