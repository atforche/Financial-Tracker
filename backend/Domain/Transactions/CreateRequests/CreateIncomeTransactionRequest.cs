using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create an <see cref="IncomeTransaction"/>
/// </summary>
public record CreateIncomeTransactionRequest : CreateTransactionRequest
{
    /// <inheritdoc/>
    public override required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override required string Location { get; init; }

    /// <inheritdoc/>
    public override required string Description { get; init; }

    /// <inheritdoc/>
    public override required decimal Amount { get; init; }

    /// <summary>
    /// Account for this Income Transaction
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Posted Date for this Income Transaction
    /// </summary>
    public DateOnly? PostedDate { get; init; }

    /// <summary>
    /// True if this transaction is the initial transaction for the account, false otherwise
    /// </summary>
    public required bool IsInitialTransactionForAccount { get; init; }
}
