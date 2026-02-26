using Domain.Transactions;

namespace Data.Transactions;

/// <summary>
/// Model representing information about a Transaction within an Account required for sorting
/// </summary>
internal sealed class AccountTransactionSortModel
{
    /// <summary>
    /// Transaction
    /// </summary>
    public required Transaction Transaction { get; init; }

    /// <summary>
    /// Account posted date for the Transaction
    /// </summary>
    public DateOnly? AccountPostedDate { get; init; }

    /// <summary>
    /// Account posted sequence for the Transaction
    /// </summary>
    public int? AccountPostedSequence { get; init; }

    /// <summary>
    /// Type for the Transaction
    /// </summary>
    public TransactionType TransactionType { get; init; }
}