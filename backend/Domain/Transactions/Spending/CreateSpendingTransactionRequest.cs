namespace Domain.Transactions.Spending;

/// <summary>
/// Record representing a request to create a <see cref="SpendingTransaction"/>
/// </summary>
public record CreateSpendingTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Source for this Spending Transaction
    /// </summary>
    public required SpendingTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<SpendingTransactionDestination> Destinations { get; init; }
}