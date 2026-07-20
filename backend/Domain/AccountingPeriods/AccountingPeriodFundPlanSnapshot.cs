using Domain.Funds;
using Domain.FundPlans;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity representing the Fund Plan configuration used within an Accounting Period.
/// </summary>
public sealed class AccountingPeriodFundPlanSnapshot : Entity<AccountingPeriodFundPlanSnapshotId>
{
    /// <summary>
    /// Fund associated with this snapshot.
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Accounting Period associated with this snapshot.
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Regular contribution configured for this Accounting Period.
    /// </summary>
    public decimal? RegularContribution { get; private set; }

    /// <summary>
    /// Minimum funded balance configured for this Accounting Period.
    /// </summary>
    public decimal? MinimumFundedBalance { get; private set; }

    /// <summary>
    /// Maximum funded balance configured for this Accounting Period.
    /// </summary>
    public decimal? MaximumFundedBalance { get; private set; }

    /// <summary>
    /// Target ending balance configured for this Accounting Period.
    /// </summary>
    public decimal? TargetEndingBalance { get; private set; }

    /// <summary>
    /// Updates the Fund Plan configuration for this Accounting Period.
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
    internal AccountingPeriodFundPlanSnapshot(
        Fund fund,
        AccountingPeriod accountingPeriod,
        FundPlan fundPlan)
        : base(new AccountingPeriodFundPlanSnapshotId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        Update(
            fundPlan.RegularContribution,
            fundPlan.MinimumFundedBalance,
            fundPlan.MaximumFundedBalance,
            fundPlan.TargetEndingBalance);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private AccountingPeriodFundPlanSnapshot()
    {
        Fund = null!;
        AccountingPeriod = null!;
    }
}

/// <summary>
/// Value object representing the ID of an <see cref="AccountingPeriodFundPlanSnapshot"/>.
/// </summary>
public sealed record AccountingPeriodFundPlanSnapshotId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodFundPlanSnapshotId(Guid value) : base(value) { }
}