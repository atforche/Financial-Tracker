using Models.AccountingPeriods;
using Models.Funds;

namespace Models.FundPlans;

/// <summary>
/// Model representing a Fund Plan for an Accounting Period.
/// </summary>
public sealed class FundPlanModel
{
    /// <summary>
    /// Gets the Fund Plan ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Fund associated with the plan.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Gets the associated Accounting Period, or null for an onboarded plan.
    /// </summary>
    public AccountingPeriodModel? AccountingPeriod { get; init; }

    /// <summary>
    /// Gets the regular contribution.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Gets the minimum funded balance.
    /// </summary>
    public decimal? MinimumFundedBalance { get; init; }

    /// <summary>
    /// Gets the maximum funded balance.
    /// </summary>
    public decimal? MaximumFundedBalance { get; init; }

    /// <summary>
    /// Gets the target ending balance.
    /// </summary>
    public decimal? TargetEndingBalance { get; init; }
}