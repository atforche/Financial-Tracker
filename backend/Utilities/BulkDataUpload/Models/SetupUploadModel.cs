using RestApi.Models.Fund;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing setup data 
/// </summary>
public class SetupUploadModel
{
    /// <summary>
    /// Funds for this Setup Model
    /// </summary>
    public required ICollection<CreateFundModel> Funds { get; init; }
}