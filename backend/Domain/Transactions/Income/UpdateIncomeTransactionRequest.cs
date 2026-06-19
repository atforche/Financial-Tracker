namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransaction"/>
/// </summary>
public record UpdateIncomeTransactionRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Income lines for this income transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeLine> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for this income transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeduction> IncomeDeductions { get; init; }

    /// <summary>
    /// Income destinations for this income transaction.
    /// </summary>
    public required IReadOnlyCollection<IncomeDestination> IncomeDestinations { get; init; }
}
