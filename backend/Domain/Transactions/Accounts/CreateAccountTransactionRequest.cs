using Domain.Accounts;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Record representing a request to create an <see cref="AccountTransaction"/>
/// </summary>
public record CreateAccountTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Debit Account for this Account Transaction
    /// </summary>
    public required Account? DebitAccount { get; init; }

    /// <summary>
    /// Credit Account for this Account Transaction
    /// </summary>
    public required Account? CreditAccount { get; init; }

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; init; }
}