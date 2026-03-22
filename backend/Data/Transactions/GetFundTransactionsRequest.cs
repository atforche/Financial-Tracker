using Domain.AccountingPeriods;

namespace Data.Transactions;

/// <summary>
/// Request to retrieve the Transactions within a Fund that match the specified criteria
/// </summary>
public record GetFundTransactionsRequest
{
    /// <summary>
    /// Accounting Period ID to filter the results by.
    /// </summary>
    public AccountingPeriodId? AccountingPeriodId { get; init; }

    /// <summary>
    /// Search to apply to the results
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Sort to apply to the results
    /// </summary>
    public FundTransactionSortOrder? Sort { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}