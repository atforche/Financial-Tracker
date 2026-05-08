using Domain.AccountingPeriods;

namespace Domain.Transactions;

/// <summary>
/// Abstract record representing a request to create a <see cref="Transaction"/>
/// </summary>
public abstract record CreateTransactionRequest
{
    /// <summary>
    /// Accounting Period ID for the Transaction
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Location for the Transaction
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Description for the Transaction
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction
    /// </summary>
    public required decimal Amount { get; init; }
}