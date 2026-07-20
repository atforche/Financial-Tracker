using Domain.Funds;
using Domain.AccountingPeriods;

namespace Domain.FundPlans;

/// <summary>
/// Entity class representing the plan for a Fund in an Accounting Period.
/// </summary>
public sealed class FundPlan : Entity<FundPlanId>
{
    /// <summary>
    /// Fund associated with this Fund Plan.
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Accounting Period associated with this Fund Plan, or null for an onboarded plan.
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; private set; }

    /// <summary>
    /// Amount normally contributed during each Accounting Period.
    /// </summary>
    public decimal? RegularContribution { get; private set; }

    /// <summary>
    /// Minimum desired balance immediately after assignments.
    /// </summary>
    public decimal? MinimumFundedBalance { get; private set; }

    /// <summary>
    /// Maximum desired balance immediately after assignments.
    /// </summary>
    public decimal? MaximumFundedBalance { get; private set; }

    /// <summary>
    /// Desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? TargetEndingBalance { get; private set; }

    /// <summary>
    /// Updates the configurable quantities for this Fund Plan.
    /// </summary>
    internal void Update(
        decimal? regularContribution,
        decimal? minimumFundedBalance,
        decimal? maximumFundedBalance,
        decimal? targetEndingBalance)
    {
        RegularContribution = regularContribution;
        MinimumFundedBalance = minimumFundedBalance;
        MaximumFundedBalance = maximumFundedBalance;
        TargetEndingBalance = targetEndingBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundPlan(
        Fund fund,
        AccountingPeriod? accountingPeriod,
        decimal? regularContribution,
        decimal? minimumFundedBalance,
        decimal? maximumFundedBalance,
        decimal? targetEndingBalance)
        : base(new FundPlanId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        Update(regularContribution, minimumFundedBalance, maximumFundedBalance, targetEndingBalance);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private FundPlan()
    {
        Fund = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="FundPlan"/>.
/// </summary>
public sealed record FundPlanId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundPlanId(Guid value) : base(value) { }
}