using Domain.Funds;

namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransaction"/>
/// </summary>
public record UpdateIncomeTransactionRequest : UpdateTransactionRequest
{
    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override required string Location { get; init; }

    /// <inheritdoc/>
    public override required string Description { get; init; }

    /// <summary>
    /// Fund Assignments for this Income Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAssignments { get; init; }

    /// <summary>
    /// Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }
}
