using Domain.Accounts;

namespace Domain.Transactions.Income;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransaction"/>
/// </summary>
public record CreateIncomeTransactionRequest : CreateTransactionRequest
{
    /// <summary>
    /// Source Account for this Income Transaction
    /// </summary>
    public required Account? SourceAccount { get; init; }

    /// <summary>
    /// External location where the money for this Income Transaction came from (if not an account).
    /// </summary>
    public required string? SourceLocation { get; init; }

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