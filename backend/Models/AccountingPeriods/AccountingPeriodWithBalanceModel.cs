namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Accounting Period along with its balance
/// </summary>
public class AccountingPeriodWithBalanceModel : AccountingPeriodModel
{
    /// <summary>
    /// Opening balance for the Accounting Period
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance for the Accounting Period
    /// </summary>
    public required decimal ClosingBalance { get; init; }
}