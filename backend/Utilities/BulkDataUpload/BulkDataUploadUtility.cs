using System.Text.Json;
using System.Text.Json.Serialization;
using RestApi;
using Utilities.BulkDataUpload.Models;
using Utilities.BulkDataUpload.Uploaders;

namespace Utilities.BulkDataUpload;

/// <summary>
/// Utility class that can bulk upload an entire accounting period's worth of data at once
/// </summary>
public partial class BulkDataUploadUtility
{
    private const string SetupFileName = "Setup.json";
    private string SetupFilePath => Path.Combine(_jsonFolderPath, SetupFileName);

    private readonly string _jsonFolderPath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="jsonFolderPath">File path of the folder with JSON files to upload</param>
    public BulkDataUploadUtility(string jsonFolderPath)
    {
        _jsonFolderPath = jsonFolderPath;
        _jsonSerializerOptions = new JsonSerializerOptions();
        _jsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Runs the Bulk Data Upload utility
    /// </summary>
    public async Task Run()
    {
        if (!Directory.Exists(_jsonFolderPath))
        {
            throw new InvalidOperationException();
        }
        if (File.Exists(SetupFilePath))
        {
            using var setupUploader = new SetupUploader();
            SetupUploadModel setupModel =
                JsonSerializer.Deserialize<SetupUploadModel>(await File.ReadAllTextAsync(SetupFilePath), _jsonSerializerOptions)
                    ?? throw new InvalidOperationException();
            await setupUploader.UploadAsync(setupModel);
        }
        using var accountingPeriodUploader = new AccountingPeriodUploader();
        foreach (string file in Directory.GetFiles(_jsonFolderPath).Where(path => path != SetupFilePath))
        {
            AccountingPeriodUploadModel accountingPeriodUploadModel =
                JsonSerializer.Deserialize<AccountingPeriodUploadModel>(await File.ReadAllTextAsync(file), _jsonSerializerOptions)
                    ?? throw new InvalidOperationException();
            await accountingPeriodUploader.UploadAsync(accountingPeriodUploadModel);
        }
    }
}