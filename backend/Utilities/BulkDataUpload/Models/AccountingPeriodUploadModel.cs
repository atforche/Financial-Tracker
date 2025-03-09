using Rest.Models.AccountingPeriod;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing an Accounting Period
/// </summary>
internal sealed class AccountingPeriodUploadModel
{
    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Is Closed flag for this Accounting Period
    /// </summary>
    public required bool IsClosed { get; init; }

    /// <summary>
    /// New Accounts for this Accounting Period
    /// </summary>
    public required ICollection<AccountUploadModel> NewAccounts { get; init; }

    /// <summary>
    /// Balance Events for this Accounting Period
    /// </summary>
    public required ICollection<BalanceEventUploadModel> BalanceEvents { get; init; }

    /// <summary>
    /// Gets a Create Accounting Period Model corresponding to this Accounting Period Upload Model
    /// </summary>
    /// <returns>A Create Accounting Period Model corresponding to this Accounting Period Upload Model</returns>
    public CreateAccountingPeriodModel GetAsCreateAccountingPeriodModel() => new()
    {
        Year = Year,
        Month = Month,
    };
}