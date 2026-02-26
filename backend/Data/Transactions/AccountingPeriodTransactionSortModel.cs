using Domain.Transactions;

namespace Data.Transactions;

/// <summary>
/// Model representing information about a Transaction within an Accounting Period required for sorting
/// </summary>
internal sealed class AccountingPeriodTransactionSortModel
{
    /// <summary>
    /// Transaction
    /// </summary>
    public required Transaction Transaction { get; init; }

    /// <summary>
    /// Name of the debit account for the Transaction
    /// </summary>
    public required string? DebitAccountName { get; init; }

    /// <summary>
    /// Name of the credit account for the Transaction
    /// </summary>
    public required string? CreditAccountName { get; init; }
}