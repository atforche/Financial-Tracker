namespace Domain.Transactions.Funds;

/// <summary>
/// Record representing a request to update a <see cref="FundTransaction"/>
/// </summary>
public record UpdateFundTransactionRequest : UpdateTransactionRequest
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
