using Domain.AccountingPeriods;

namespace Domain.Transactions.CreateRequests;

/// <summary>
/// Abstract record representing a request to create a <see cref="Transaction"/>
/// </summary>
public abstract record CreateTransactionRequest
{
    /// <summary>
    /// Accounting Period ID for the Transaction
    /// </summary>
    public abstract AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public abstract DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Location for the Transaction
    /// </summary>
    public abstract string Location { get; init; }

    /// <summary>
    /// Description for the Transaction
    /// </summary>
    public abstract string Description { get; init; }

    /// <summary>
    /// Amount for the Transaction
    /// </summary>
    public abstract decimal Amount { get; init; }
}
