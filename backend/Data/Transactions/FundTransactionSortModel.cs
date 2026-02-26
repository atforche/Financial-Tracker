using Domain.Transactions;

namespace Data.Transactions;

/// <summary>
/// Model representing information about a Transaction within a Fund required for sorting
/// </summary>
internal sealed class FundTransactionSortModel
{
    /// <summary>
    /// Transaction
    /// </summary>
    public required Transaction Transaction { get; init; }

    /// <summary>
    /// Type for the Transaction
    /// </summary>
    public TransactionType TransactionType { get; init; }
}