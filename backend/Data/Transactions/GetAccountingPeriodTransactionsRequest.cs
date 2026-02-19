using Domain.Accounts;

namespace Data.Transactions;

/// <summary>
/// Request to retrieve the Transactions within an Accounting Period that match the specified criteria
/// </summary>
public record GetAccountingPeriodTransactionsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountingPeriodTransactionSortOrder? SortBy { get; init; }

    /// <summary>
    /// Minimum date to include in the results
    /// </summary>
    public DateOnly? MinDate { get; init; }

    /// <summary>
    /// Maximum date to include in the results
    /// </summary>
    public DateOnly? MaxDate { get; init; }

    /// <summary>
    /// Locations to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Locations { get; init; }

    /// <summary>
    /// Accounts to include in the results (either as debit or credit accounts)
    /// </summary>
    public IReadOnlyCollection<AccountId>? Account { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}