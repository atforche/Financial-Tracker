namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransaction"/>
/// </summary>
public record CreateIncomeTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Source for this Income Transaction
    /// </summary>
    public required IncomeTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<IncomeTransactionDestination> Destinations { get; init; }
}