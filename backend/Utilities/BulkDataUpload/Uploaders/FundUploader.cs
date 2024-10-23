using System.Net.Http.Json;
using RestApi.Models.Fund;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Uploader class responsible for uploading Funds to the REST API
/// </summary>
public class FundUploader : DataUploader
{
    private readonly IEnumerable<CreateFundModel> _models;

    /// <summary>
    /// List of Fund Models that were uploaded
    /// </summary>
    public ICollection<FundModel> Results { get; } = [];

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="models">Create Fund Models to upload to the REST API</param>
    public FundUploader(IEnumerable<CreateFundModel> models)
    {
        _models = models;
    }

    /// <inheritdoc/>
    public override async Task UploadAsync()
    {
        foreach (CreateFundModel model in _models)
        {
            await ApiErrorHandlingWrapper(async () =>
            {
                Console.WriteLine($"Uploading fund: {model.Name}");
                UriBuilder.Path = "/funds";
                HttpResponseMessage response = await Client.PostAsJsonAsync(UriBuilder.Uri, model);
                response.EnsureSuccessStatusCode();
                Results.Add(await response.Content.ReadFromJsonAsync<FundModel>()
                    ?? throw new InvalidOperationException());
            },
            $"Unable to upload fund {model.Name}");
        }
    }
}