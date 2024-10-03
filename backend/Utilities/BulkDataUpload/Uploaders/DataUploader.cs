using System.Net.Http.Headers;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Base class responsible for uploading data to the REST API
/// </summary>
public abstract class DataUploader : IDisposable
{
    /// <summary>
    /// HTTP client for this uploader to use
    /// </summary>
    protected HttpClient Client { get; init; }

    /// <summary>
    /// URI builder for this uploader to use
    /// </summary>
    protected UriBuilder UriBuilder { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    protected DataUploader()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        UriBuilder = new UriBuilder("http", "localhost", 8080);
    }

    /// <summary>
    /// Uploads the provided data to the REST API
    /// </summary>
    public abstract Task UploadAsync();

    /// <summary>
    /// Wrapper method that wraps an API call with error handling
    /// </summary>
    /// <param name="functionToWrap">Async function call to wrap with error handling</param>
    /// <param name="failureDescription">Message to display if an error is encountered</param>
    protected static async Task ApiErrorHandlingWrapper(Func<Task> functionToWrap, string failureDescription)
    {
        try
        {
            await functionToWrap();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Exception encountered: {failureDescription}");
            Console.WriteLine(e.Message);
            Console.WriteLine();
        }
    }

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
            Client.Dispose();
        }
    }
}