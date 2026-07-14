using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for getting Accounting Periods with their transactions.
/// </summary>
public class AccountingPeriodWithTransactionsQueryParameterModel
{
    /// <summary>
    /// Optional sort to apply to the matching transactions.
    /// </summary>
    public TransactionSortOrderModel? TransactionSort { get; init; }

    /// <summary>
    /// Pagination settings for the matching transactions.
    /// </summary>
    public PaginationModel? TransactionPagination { get; init; }
}