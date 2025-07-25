using Rest.Models.Accounts;
using Rest.Models.FundConversions;
using Rest.Models.Funds;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing a Fund Conversion Balance Event
/// </summary>
internal sealed class FundConversionUploadModel : BalanceEventUploadModel
{
    /// <summary>
    /// Account Name for this Fund Conversion Balance Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Fund Conversion Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// From Fund Name for this Fund Conversion Balance Event
    /// </summary>
    public required string FromFundName { get; init; }

    /// <summary>
    /// To Fund Name for this Fund Conversion Balance Event
    /// </summary>
    public required string ToFundName { get; init; }

    /// <summary>
    /// Amount for this Fund Conversion Balance Event
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Gets a Create Fund Conversion Model corresponding to this Fund Conversion Upload Model
    /// </summary>
    /// <param name="existingFunds">List of existing Funds</param>
    /// <param name="existingAccounts">List of existing Accounts</param>
    /// <returns>A Create Fund Conversion Model corresponding to this Fund Conversion Upload Model</returns>
    public CreateFundConversionModel GetAsCreateFundConversionModel(
        ICollection<FundModel> existingFunds,
        ICollection<AccountModel> existingAccounts) => new()
        {
            AccountingPeriodId = Guid.NewGuid(),
            AccountId = existingAccounts.Single(account => account.Name == AccountName).Id,
            EventDate = EventDate,
            FromFundId = existingFunds.Single(fund => fund.Name == FromFundName).Id,
            ToFundId = existingFunds.Single(fund => fund.Name == ToFundName).Id,
            Amount = Amount,
        };
}