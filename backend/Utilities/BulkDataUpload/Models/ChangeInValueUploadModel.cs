using Rest.Models.Accounts;
using Rest.Models.ChangeInValues;
using Rest.Models.Funds;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing a Change In Value Balance Event
/// </summary>
internal sealed class ChangeInValueUploadModel : BalanceEventUploadModel
{
    /// <summary>
    /// Account Name for this Change In Value Balance Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Change In Value Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Fund Amount for this Change In Value Balance Event
    /// </summary>
    public required FundAmountUploadModel FundAmount { get; init; }

    /// <summary>
    /// Gets a Create Change In Value Model corresponding to this Change In Value Upload Model
    /// </summary>
    /// <param name="existingFunds">List of existing Funds</param>
    /// <param name="existingAccounts">List of existing Accounts</param>
    /// <returns>A Create Change In Value Model corresponding to the Change In Value Upload Model</returns>
    public CreateChangeInValueModel GetAsCreateChangeInValueModel(
        ICollection<FundModel> existingFunds,
        ICollection<AccountModel> existingAccounts) => new()
        {
            AccountingPeriodId = Guid.NewGuid(),
            AccountId = existingAccounts.Single(account => account.Name == AccountName).Id,
            EventDate = EventDate,
            FundAmount = FundAmount.GetAsCreateFundAmountModel(existingFunds)
        };
}