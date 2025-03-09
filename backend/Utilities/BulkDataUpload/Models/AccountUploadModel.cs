using Domain.Aggregates.Accounts;
using Rest.Models.Account;
using Rest.Models.Fund;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing an Account
/// </summary>
internal sealed class AccountUploadModel
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Starting Fund Balances for this Account
    /// </summary>
    public required ICollection<FundAmountUploadModel> StartingFundBalances { get; init; }

    /// <summary>
    /// Gets a Create Account Model corresponding to this Account Upload Model
    /// </summary>
    /// <param name="existingFunds">List of existing funds</param>
    /// <returns>A Create Account Model corresponding to this Account Upload Model</returns>
    public CreateAccountModel GetAsCreateAccountModel(ICollection<FundModel> existingFunds) => new()
    {
        Name = Name,
        Type = Type,
        StartingFundBalances = StartingFundBalances
            .Select(fundAmount => fundAmount.GetAsCreateFundAmountModel(existingFunds)).ToList(),
    };
}