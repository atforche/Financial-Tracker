namespace Domain.Transactions.Accounts;

/// <summary>
/// Record representing a request to update a <see cref="AccountTransaction"/>
/// </summary>
public record UpdateAccountTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Source for this Account Transaction
    /// </summary>
    public required AccountTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Account Transaction
    /// </summary>
    public required IReadOnlyCollection<AccountTransactionDestination> Destinations { get; init; }
}
