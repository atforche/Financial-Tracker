namespace Domain.Transactions.Accounts;

/// <summary>
/// Record representing a request to update a <see cref="AccountTransaction"/>
/// </summary>
public record UpdateAccountTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Posted Date for the Debit Account of this Account Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Account Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}