using System.Net.Http.Json;
using RestApi.Models.AccountingPeriod;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Uploader class responsible for uploading Transactions to the REST API
/// </summary>
public class TransactionUploader : DataUploader
{
    private readonly AccountingPeriodModel _accountingPeriod;
    private readonly IEnumerable<CreateTransactionModel> _models;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period that was uploaded</param>
    /// <param name="models">Create Transaction Models to upload to the REST API</param>
    public TransactionUploader(AccountingPeriodModel accountingPeriod, IEnumerable<CreateTransactionModel> models)
    {
        _accountingPeriod = accountingPeriod;
        _models = models;
    }

    /// <inheritdoc/>
    public override async Task UploadAsync()
    {
        int i = 1;
        foreach (CreateTransactionModel model in _models)
        {
            await ApiErrorHandlingWrapper(async () =>
            {
                Console.WriteLine($"Uploading transaction: {i}");
                UriBuilder.Path = $"/accountingPeriod/{_accountingPeriod.Id}/Transactions";
                HttpResponseMessage response = await Client.PostAsJsonAsync(UriBuilder.Uri, model);
                response.EnsureSuccessStatusCode();
            },
            $"Unable to upload transaction {i}");
            ++i;
        }
    }
}