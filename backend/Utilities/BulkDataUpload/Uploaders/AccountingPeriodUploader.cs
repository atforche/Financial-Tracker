using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestApi.Models.AccountingPeriod;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Uploader class responsible for uploading an Accounting Period to the REST API
/// </summary>
public class AccountingPeriodUploader : DataUploader
{
    private readonly CreateAccountingPeriodModel _model;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Accounting Period Model that was uploaded 
    /// </summary>
    public AccountingPeriodModel? Result { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="model">Create Accounting Period Model to upload to the REST API</param>
    public AccountingPeriodUploader(CreateAccountingPeriodModel model)
    {
        _model = model;
        _serializerOptions = new JsonSerializerOptions();
        _serializerOptions.PropertyNameCaseInsensitive = true;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc/>
    public override async Task UploadAsync() =>
        await ApiErrorHandlingWrapper(async () =>
        {
            Console.WriteLine($"Uploading accounting period: {_model.Year}-{_model.Month}");
            UriBuilder.Path = "/accountingPeriod";
            HttpResponseMessage response = await Client.PostAsJsonAsync(UriBuilder.Uri, _model);
            response.EnsureSuccessStatusCode();
            Result = await response.Content.ReadFromJsonAsync<AccountingPeriodModel>();
        },
        $"Unable to upload accounting period {_model.Year}-{_model.Month}");

    /// <summary>
    /// Closes the Accounting Period that was uploaded
    /// </summary>
    public async Task CloseUploadedPeriodAsync()
    {
        if (Result == null)
        {
            throw new InvalidOperationException();
        }
        await ApiErrorHandlingWrapper(async () =>
        {
            Console.WriteLine($"Closing accounting period: {_model.Year}-{_model.Month}");
            UriBuilder.Path = $"/accountingPeriod/close/{Result.Id.ToString()}";
            HttpResponseMessage response = await Client.PostAsync(UriBuilder.Uri, null);
            response.EnsureSuccessStatusCode();
            Result = await response.Content.ReadFromJsonAsync<AccountingPeriodModel>();
        },
        $"Unable to close accounting period {_model.Year}-{_model.Month}");
    }
}