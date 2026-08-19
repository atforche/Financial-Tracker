using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundGoals;

/// <summary>
/// Persisted unposted Transaction effect for Fund Goal totals.
/// </summary>
public sealed class PendingFundGoalTotalsEffect : Entity<PendingFundGoalTotalsEffectId>
{
    /// <summary>
    /// Fund affected by this pending effect.
    /// </summary>
    public FundId FundId { get; init; }

    /// <summary>
    /// Accounting Period containing the pending effect.
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Transaction that created this pending effect.
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Pending amount assigned for the Fund Goal.
    /// </summary>
    public decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Pending amount assigned toward the regular monthly contribution.
    /// </summary>
    public decimal PendingRegularAmountAssigned { get; init; }

    /// <summary>
    /// Pending amount spent for the Fund Goal.
    /// </summary>
    public decimal PendingAmountSpent { get; init; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal PendingFundGoalTotalsEffect(
        FundId fundId,
        AccountingPeriodId accountingPeriodId,
        TransactionId transactionId,
        decimal pendingAmountAssigned,
        decimal pendingRegularAmountAssigned,
        decimal pendingAmountSpent)
        : base(new PendingFundGoalTotalsEffectId(Guid.NewGuid()))
    {
        FundId = fundId;
        AccountingPeriodId = accountingPeriodId;
        TransactionId = transactionId;
        PendingAmountAssigned = pendingAmountAssigned;
        PendingRegularAmountAssigned = pendingRegularAmountAssigned;
        PendingAmountSpent = pendingAmountSpent;
    }

    /// <summary>
    /// Constructs a new instance of this class for EF Core.
    /// </summary>
    private PendingFundGoalTotalsEffect()
    {
        FundId = null!;
        AccountingPeriodId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// ID for a <see cref="PendingFundGoalTotalsEffect"/>.
/// </summary>
public sealed record PendingFundGoalTotalsEffectId : EntityId
{
    internal PendingFundGoalTotalsEffectId(Guid value) : base(value) { }
}
