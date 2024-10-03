using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestApi.Models.Account;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Uploader class responsible for uploading Accounts to the REST API
/// </summary>
public class AccountUploader : DataUploader
{
    private readonly IEnumerable<CreateAccountModel> _models;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// List of Account Models that were uploaded
    /// </summary>
    public ICollection<AccountModel> Results { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="models">Create Account Models to upload to the REST API</param>
    public AccountUploader(IEnumerable<CreateAccountModel> models)
    {
        _models = models;
        _serializerOptions = new JsonSerializerOptions();
        _serializerOptions.PropertyNameCaseInsensitive = true;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        Results = [];
    }

    /// <inheritdoc/>
    public override async Task UploadAsync()
    {
        foreach (CreateAccountModel model in _models)
        {
            await ApiErrorHandlingWrapper(async () =>
            {
                Console.WriteLine($"Uploading account: {model.Name}");
                UriBuilder.Path = "/accounts";
                HttpResponseMessage response = await Client.PostAsJsonAsync(UriBuilder.Uri, model);
                response.EnsureSuccessStatusCode();
                Results.Add(await response.Content.ReadFromJsonAsync<AccountModel>(_serializerOptions)
                    ?? throw new InvalidOperationException());
            },
            $"Unable to upload account {model.Name}");
        }
    }
}