using Domain.Accounts;

namespace Domain.Transactions.AccountTransfer;

/// <summary>
/// Record representing a request to create an <see cref="AccountTransferTransaction"/>
/// </summary>
public record CreateAccountTransferTransactionRequest : CreateTransactionRequest
{

    /// <summary>
    /// Debit Account for this Account Transfer Transaction
    /// </summary>
    public required Account DebitAccount { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Credit Account for this Account Transfer Transaction
    /// </summary>
    public required Account CreditAccount { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Account Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}