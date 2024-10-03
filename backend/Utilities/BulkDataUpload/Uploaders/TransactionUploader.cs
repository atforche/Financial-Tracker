using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestApi.Models.Account;
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

/// <summary>
/// JSON converter class to convert an Account's name into its associated GUID
/// </summary>
public class AccountNameIdConverter : JsonConverter<Guid>
{
    private readonly Dictionary<string, Guid> _accounts;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accounts">List of accounts that have been created</param>
    public AccountNameIdConverter(IEnumerable<AccountModel> accounts)
    {
        _accounts = accounts.ToDictionary(model => model.Name, model => model.Id);
    }

    /// <inheritdoc/>
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        _accounts[reader.GetString() ?? ""];

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) =>
        writer.WriteStringValue(_accounts.First(pair => pair.Value == value).Key);
}