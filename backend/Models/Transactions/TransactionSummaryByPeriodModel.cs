using Models.AccountingPeriods;

namespace Models.Transactions;

/// <summary>
/// Model representing a summary of transactions for a specific accounting period.
/// </summary>
public class TransactionSummaryByPeriodModel : TransactionSummaryModel
{
    /// <summary>
    /// Accounting Period.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }
}