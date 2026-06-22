using Domain.Accounts;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Record representing a request to create an <see cref="AccountTransaction"/>
/// </summary>
public record CreateAccountTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Source for this Account Transaction
    /// </summary>
    public required AccountTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Account Transaction
    /// </summary>
    public required IReadOnlyCollection<AccountTransactionDestination> Destinations { get; init; }

    /// <summary>
    /// Account ID of the Account that generated this transaction when it was created, or null
    /// </summary>
    public AccountId? GeneratedByAccountId { get; init; }
}