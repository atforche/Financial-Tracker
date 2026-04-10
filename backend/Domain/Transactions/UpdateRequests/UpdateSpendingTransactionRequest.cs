using Domain.Funds;

namespace Domain.Transactions.UpdateRequests;

/// <summary>
/// Record representing a request to update a <see cref="SpendingTransaction"/>
/// </summary>
public record UpdateSpendingTransactionRequest : UpdateTransactionRequest
{
    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override required string Location { get; init; }

    /// <inheritdoc/>
    public override required string Description { get; init; }

    /// <summary>
    /// Fund Amounts for this Spending Transaction
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundAmounts { get; init; }

    /// <summary>
    /// Posted Date for this Spending Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }
}
