using RestApi.Models.Fund;
using RestApi.Models.FundAmount;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Buld data upload model representing a Fund Amount
/// </summary>
public class FundAmountUploadModel
{
    /// <summary>
    /// Fund Name for this Fund Amount
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Gets a Create Fund Amount Model corresponding to this Fund Amount Upload Model
    /// </summary>
    /// <param name="existingFunds">List of existing funds</param>
    /// <returns>A Create Fund Amount Model corresponding to this Fund Amount Upload Model</returns>
    public CreateFundAmountModel GetAsCreateFundAmountModel(ICollection<FundModel> existingFunds) =>
        new CreateFundAmountModel
        {
            FundId = existingFunds.Single(fund => fund.Name == FundName).Id,
            Amount = Amount
        };
}