using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.FundPlans;

/// <summary>
/// Persisted unposted Transaction effect for Fund Plan totals.
/// </summary>
public sealed class PendingFundPlanTotalsEffect : Entity<PendingFundPlanTotalsEffectId>
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
    /// Pending amount assigned for the Fund Plan.
    /// </summary>
    public decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Pending amount spent for the Fund Plan.
    /// </summary>
    public decimal PendingAmountSpent { get; init; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal PendingFundPlanTotalsEffect(FundId fundId, AccountingPeriodId accountingPeriodId, TransactionId transactionId, decimal pendingAmountAssigned, decimal pendingAmountSpent)
        : base(new PendingFundPlanTotalsEffectId(Guid.NewGuid()))
    {
        FundId = fundId;
        AccountingPeriodId = accountingPeriodId;
        TransactionId = transactionId;
        PendingAmountAssigned = pendingAmountAssigned;
        PendingAmountSpent = pendingAmountSpent;
    }

    /// <summary>
    /// Constructs a new instance of this class for EF Core.
    /// </summary>
    private PendingFundPlanTotalsEffect()
    {
        FundId = null!;
        AccountingPeriodId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// ID for a <see cref="PendingFundPlanTotalsEffect"/>.
/// </summary>
public sealed record PendingFundPlanTotalsEffectId : EntityId
{
    internal PendingFundPlanTotalsEffectId(Guid value) : base(value) { }
}