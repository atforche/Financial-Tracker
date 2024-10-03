using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using RestApi;
using RestApi.Models.Account;
using RestApi.Models.AccountingPeriod;
using RestApi.Models.Transaction;
using Utilities.BulkDataUpload.Uploaders;

namespace Utilities.BulkDataUpload;

/// <summary>
/// Utility class that can bulk upload an entire accounting period's worth of data at once
/// </summary>
public partial class BulkDataUploadUtility
{
    private readonly string _jsonFilePath;
    private readonly JsonSerializerOptions _defaultSerializerOptions;
    private readonly JsonSerializerOptions _accountSerializerOptions;
    private readonly JsonSerializerOptions _transactionSerializerOptions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="jsonFilePath">File path of the JSON file to upload</param>
    public BulkDataUploadUtility(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
        _defaultSerializerOptions = new JsonSerializerOptions();
        _accountSerializerOptions = new JsonSerializerOptions();
        _accountSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        _transactionSerializerOptions = new JsonSerializerOptions();
        _transactionSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    }

    /// <summary>
    /// Runs the Bulk Data Upload utility
    /// </summary>
    public async Task Run()
    {
        if (!File.Exists(_jsonFilePath))
        {
            throw new InvalidOperationException();
        }
        string fileContents = await File.ReadAllTextAsync(_jsonFilePath);
        List<string> sections = GetUploadSectionRegex().Split(fileContents).Skip(1).ToList();

        CreateAccountingPeriodModel accountingPeriod = JsonSerializer.Deserialize<CreateAccountingPeriodModel>(
            sections[0], _defaultSerializerOptions) ?? throw new InvalidOperationException();
        using var accountingPeriodUploader = new AccountingPeriodUploader(accountingPeriod);
        await accountingPeriodUploader.UploadAsync();

        IEnumerable<CreateAccountModel> accounts = JsonSerializer.Deserialize<IEnumerable<CreateAccountModel>>(
            sections[1], _accountSerializerOptions) ?? throw new InvalidOperationException();
        using var accountUploader = new AccountUploader(accounts);
        await accountUploader.UploadAsync();

        _transactionSerializerOptions.Converters.Add(new AccountNameIdConverter(accountUploader.Results));
        IEnumerable<CreateTransactionModel> transactions = JsonSerializer.Deserialize<IEnumerable<CreateTransactionModel>>(
            sections[2], _transactionSerializerOptions) ?? throw new InvalidOperationException();
        using var transactionUploader = new TransactionUploader(transactions);
        await transactionUploader.UploadAsync();

        await accountingPeriodUploader.CloseUploadedPeriodAsync();
    }

    [GeneratedRegex("// ----- .* -----")]
    private static partial Regex GetUploadSectionRegex();
}