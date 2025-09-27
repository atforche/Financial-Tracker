using Rest.Models.Funds;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing setup data 
/// </summary>
internal sealed class SetupUploadModel
{
    /// <summary>
    /// Funds for this Setup Model
    /// </summary>
    public required ICollection<CreateOrUpdateFundModel> Funds { get; init; }
}