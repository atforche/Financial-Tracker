using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Entity representing Fund Goal assignment and spending totals after a Transaction.
/// </summary>
public sealed class FundGoalTotalsHistory : Entity<FundGoalTotalsHistoryId>
{
    /// <summary>
    /// Fund associated with these totals.
    /// </summary>
    public FundId FundId { get; init; }

    /// <summary>
    /// Accounting Period in which these totals apply.
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Transaction that produced this history entry.
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Transaction date used to order this history entry.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Transaction sequence used to order this history entry.
    /// </summary>
    public int Sequence { get; init; }

    /// <summary>
    /// Posted amount assigned after the Transaction.
    /// </summary>
    public decimal AmountAssigned { get; private set; }

    /// <summary>
    /// Posted amount spent after the Transaction.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Updates the persisted totals.
    /// </summary>
    public void Update(FundGoalTotals totals)
    {
        if (totals.FundId != FundId)
        {
            throw new InvalidOperationException("Cannot update Fund Goal totals history for a different Fund.");
        }
        AmountAssigned = totals.AmountAssigned;
        AmountSpent = totals.AmountSpent;
    }

    /// <summary>
    /// Converts this history entry to Fund Goal totals.
    /// </summary>
    public FundGoalTotals ToTotals() =>
        new(FundId, AmountAssigned, AmountSpent);

    /// <summary>
    /// Constructs a Fund Goal totals history entry.
    /// </summary>
    internal FundGoalTotalsHistory(FundId fundId, AccountingPeriodId accountingPeriodId, Transaction transaction, FundGoalTotals totals)
        : base(new FundGoalTotalsHistoryId(Guid.NewGuid()))
    {
        FundId = fundId;
        AccountingPeriodId = accountingPeriodId;
        TransactionId = transaction.Id;
        Date = transaction.Date;
        Sequence = transaction.Sequence;
        Update(totals);
    }

    /// <summary>
    /// Constructs a default instance for persistence.
    /// </summary>
    private FundGoalTotalsHistory()
    {
        FundId = null!;
        AccountingPeriodId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object representing the ID of a Fund Goal totals history entry.
/// </summary>
public sealed record FundGoalTotalsHistoryId : EntityId
{
    /// <summary>
    /// Constructs a Fund Goal totals history ID.
    /// </summary>
    internal FundGoalTotalsHistoryId(Guid value) : base(value) { }
}