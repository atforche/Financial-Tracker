using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestApi;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Base class responsible for uploading data to the REST API
/// </summary>
public abstract class DataUploader<T> : IDisposable
{
    private readonly HttpClient _client;
    private readonly UriBuilder _uriBuilder;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    protected DataUploader()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _uriBuilder = new UriBuilder("http", "localhost", 8080);
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        _jsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Uploads the provided model to the REST API
    /// </summary>
    /// <param name="model">Model to upload</param>
    public abstract Task UploadAsync(T model);

    /// <summary>
    /// Performs an HTTP GET operation using the provided URI path and returns a result of the provided type
    /// </summary>
    /// <param name="endpointPath">Endpoint path to perform the HTTP GET operation against</param>
    /// <returns>The result of the GET operation cast to the provided type</returns>
    protected async Task<TResult> GetAsync<TResult>(string endpointPath)
    {
        _uriBuilder.Path = endpointPath;
        HttpResponseMessage response = await _client.GetAsync(_uriBuilder.Uri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResult>(_jsonSerializerOptions) ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Performs an HTTP POST operation using the provided URI path and body model. Results a result of the provided type.
    /// </summary>
    /// <param name="endpointPath">Endpoint path to perform the HTTP POST operation against</param>
    /// <param name="bodyModel">Model to pass as the body of the HTTP POST request</param>
    /// <returns>The result of the POST operation cast to the provided type</returns>
    protected async Task<TResult> PostAsync<TBody, TResult>(string endpointPath, TBody bodyModel)
    {
        _uriBuilder.Path = endpointPath;
        HttpResponseMessage response = await _client.PostAsJsonAsync(_uriBuilder.Uri, bodyModel);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResult>(_jsonSerializerOptions) ?? throw new InvalidOperationException();
    }

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose any resources associated with this DataUploader
    /// </summary>
    /// <param name="disposing">True if we're in the Dispose method, false otherwise</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client.Dispose();
        }
    }

    #endregion
}