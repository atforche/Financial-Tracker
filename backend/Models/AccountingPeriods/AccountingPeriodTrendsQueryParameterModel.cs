using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for the Accounting Period trends endpoint.
/// </summary>
public class AccountingPeriodTrendsQueryParameterModel
{
    /// <summary>
    /// ID for the first Accounting Period in the requested range.
    /// </summary>
    public required Guid StartAccountingPeriodId { get; init; }

    /// <summary>
    /// ID for the last Accounting Period in the requested range.
    /// </summary>
    public required Guid EndAccountingPeriodId { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching Accounting Periods.
    /// </summary>
    public AccountingPeriodSortOrderModel? Sort { get; init; }

    /// <summary>
    /// Optional sort to apply to the matching transactions.
    /// </summary>
    public TransactionSortOrderModel? TransactionSort { get; init; }

    /// <summary>
    /// Maximum number of Accounting Periods to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of Accounting Periods to skip.
    /// </summary>
    public int? Offset { get; init; }

    /// <summary>
    /// Maximum number of transactions to return.
    /// </summary>
    public int? TransactionLimit { get; init; }

    /// <summary>
    /// Number of transactions to skip.
    /// </summary>
    public int? TransactionOffset { get; init; }
}