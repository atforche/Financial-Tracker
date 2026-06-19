using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for the current Accounting Period endpoint.
/// </summary>
public class CurrentAccountingPeriodQueryParameterModel
{
    /// <summary>
    /// Optional sort to apply to the matching transactions.
    /// </summary>
    public TransactionSortOrderModel? TransactionSort { get; init; }

    /// <summary>
    /// Maximum number of transactions to return.
    /// </summary>
    public int? TransactionLimit { get; init; }

    /// <summary>
    /// Number of transactions to skip.
    /// </summary>
    public int? TransactionOffset { get; init; }
}