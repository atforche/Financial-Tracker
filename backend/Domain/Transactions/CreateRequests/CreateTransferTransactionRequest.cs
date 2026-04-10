using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create a <see cref="TransferTransaction"/>
/// </summary>
public record CreateTransferTransactionRequest : CreateTransactionRequest
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
    /// Debit Account for this Transfer Transaction
    /// </summary>
    public required Account DebitAccount { get; init; }

    /// <summary>
    /// Posted Date for the Debit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Credit Account for this Transfer Transaction
    /// </summary>
    public required Account CreditAccount { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Transfer Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}
