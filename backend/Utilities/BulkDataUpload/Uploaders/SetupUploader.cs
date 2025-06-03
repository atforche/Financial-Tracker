using Rest.Models.Funds;
using Utilities.BulkDataUpload.Models;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Bulk data uploader that uploads setup data to the REST API
/// </summary>
internal sealed class SetupUploader : DataUploader<SetupUploadModel>
{
    /// <inheritdoc/>
    public override async Task UploadAsync(SetupUploadModel model)
    {
        foreach (CreateFundModel createFundModel in model.Funds)
        {
            Console.WriteLine($"Uploading Fund: {createFundModel.Name}");
            _ = await PostAsync<CreateFundModel, FundModel>("/funds", createFundModel);
        }
    }
}