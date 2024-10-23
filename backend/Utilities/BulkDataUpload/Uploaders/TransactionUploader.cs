using System.Net.Http.Json;
using RestApi.Models.Transaction;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Uploader class responsible for uploading Transactions to the REST API
/// </summary>
public class TransactionUploader : DataUploader
{
    private readonly IEnumerable<CreateTransactionModel> _models;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="models">Create Transaction Models to upload to the REST API</param>
    public TransactionUploader(IEnumerable<CreateTransactionModel> models)
    {
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
                UriBuilder.Path = "/transactions";
                HttpResponseMessage response = await Client.PostAsJsonAsync(UriBuilder.Uri, model);
                response.EnsureSuccessStatusCode();
            },
            $"Unable to upload transaction {i}");
            ++i;
        }
    }
}