using Domain.AccountingPeriods;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Record representing a request to create a <see cref="RefundTransaction"/>
/// </summary>
public record CreateRefundTransactionRequest(Transaction Transaction) : CreateTransactionRequest
{
    /// <inheritdoc/>
    public override AccountingPeriodId AccountingPeriodId { get; init; } = Transaction.AccountingPeriodId;

    /// <inheritdoc/>
    public override required DateOnly TransactionDate { get; init; }

    /// <inheritdoc/>
    public override string Location { get; init; } = Transaction.Location;

    /// <inheritdoc/>
    public override string Description { get; init; } = Transaction.Description;

    /// <inheritdoc/>
    public override decimal Amount { get; init; } = Transaction.Amount;

    /// <summary>
    /// Posted Date for the Debit Account of this Refund Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }

    /// <summary>
    /// Posted Date for the Credit Account of this Refund Transaction
    /// </summary>
    public DateOnly? CreditPostedDate { get; init; }
}
