namespace Models.Accounts;

/// <summary>
/// Model representing top-level trends balances for a specific Accounting Period.
/// </summary>
public class AccountTrendsPeriodSummaryModel
{
    /// <summary>
    /// ID for the Accounting Period.
    /// </summary>
    public required Guid AccountingPeriodId { get; init; }

    /// <summary>
    /// Name for the Accounting Period.
    /// </summary>
    public required string AccountingPeriodName { get; init; }

    /// <summary>
    /// Year for the Accounting Period.
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for the Accounting Period.
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Total opening balance across all matching Accounts.
    /// </summary>
    public required decimal TotalOpeningBalance { get; init; }

    /// <summary>
    /// Total closing balance across all matching Accounts.
    /// </summary>
    public required decimal TotalClosingBalance { get; init; }

    /// <summary>
    /// Opening balance across tracked Accounts.
    /// </summary>
    public required decimal TrackedOpeningBalance { get; init; }

    /// <summary>
    /// Closing balance across tracked Accounts.
    /// </summary>
    public required decimal TrackedClosingBalance { get; init; }

    /// <summary>
    /// Opening balance across untracked Accounts.
    /// </summary>
    public required decimal UntrackedOpeningBalance { get; init; }

    /// <summary>
    /// Closing balance across untracked Accounts.
    /// </summary>
    public required decimal UntrackedClosingBalance { get; init; }

    /// <summary>
    /// Opening balance totals grouped by Account Type.
    /// </summary>
    public required IReadOnlyCollection<AccountTypeBalanceModel> OpeningBalanceByAccountType { get; init; }

    /// <summary>
    /// Closing balance totals grouped by Account Type.
    /// </summary>
    public required IReadOnlyCollection<AccountTypeBalanceModel> ClosingBalanceByAccountType { get; init; }
}