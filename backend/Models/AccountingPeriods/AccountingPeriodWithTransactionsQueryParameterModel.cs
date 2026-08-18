using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the query parameters for getting Accounting Periods with their transactions.
/// </summary>
public class AccountingPeriodWithTransactionsQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Optional sort to apply to the matching transactions.
    /// </summary>
    public TransactionSortModel? Sort { get; init; }
}
