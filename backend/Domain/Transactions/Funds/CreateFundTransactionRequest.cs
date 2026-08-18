namespace Domain.Transactions.Funds;

/// <summary>
/// Record representing a request to create an <see cref="FundTransaction"/>
/// </summary>
public record CreateFundTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Source for this Fund Transaction
    /// </summary>
    public required FundTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Fund Transaction
    /// </summary>
    public required IReadOnlyCollection<FundTransactionDestination> Destinations { get; init; }
}
