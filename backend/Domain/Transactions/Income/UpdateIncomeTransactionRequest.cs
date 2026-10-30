namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransaction"/>
/// </summary>
public record UpdateIncomeTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Source for this Income Transaction
    /// </summary>
    public required IncomeTransactionSource Source { get; init; }

    /// <summary>
    /// Destinations for this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<IncomeTransactionDestination> Destinations { get; init; }

    /// <summary>
    /// ID of the Expected Income Source this income was planned as, or null when the income is unplanned
    /// </summary>
    public required Guid? ExpectedIncomeSourceId { get; init; }
}
