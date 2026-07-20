using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Entity representing Fund Plan assignment and spending totals after a Transaction.
/// </summary>
public sealed class FundPlanTotalsHistory : Entity<FundPlanTotalsHistoryId>
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
    /// Pending amount assigned after the Transaction.
    /// </summary>
    public decimal PendingAmountAssigned { get; private set; }

    /// <summary>
    /// Posted amount spent after the Transaction.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Pending amount spent after the Transaction.
    /// </summary>
    public decimal PendingAmountSpent { get; private set; }

    /// <summary>
    /// Updates the persisted totals.
    /// </summary>
    public void Update(FundPlanTotals totals)
    {
        if (totals.FundId != FundId)
        {
            throw new InvalidOperationException("Cannot update Fund Plan totals history for a different Fund.");
        }
        AmountAssigned = totals.AmountAssigned;
        PendingAmountAssigned = totals.PendingAmountAssigned;
        AmountSpent = totals.AmountSpent;
        PendingAmountSpent = totals.PendingAmountSpent;
    }

    /// <summary>
    /// Converts this history entry to Fund Plan totals.
    /// </summary>
    public FundPlanTotals ToTotals() =>
        new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Constructs a Fund Plan totals history entry.
    /// </summary>
    internal FundPlanTotalsHistory(FundId fundId, AccountingPeriodId accountingPeriodId, Transaction transaction, FundPlanTotals totals)
        : base(new FundPlanTotalsHistoryId(Guid.NewGuid()))
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
    private FundPlanTotalsHistory()
    {
        FundId = null!;
        AccountingPeriodId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object representing the ID of a Fund Plan totals history entry.
/// </summary>
public sealed record FundPlanTotalsHistoryId : EntityId
{
    /// <summary>
    /// Constructs a Fund Plan totals history ID.
    /// </summary>
    internal FundPlanTotalsHistoryId(Guid value) : base(value) { }
}