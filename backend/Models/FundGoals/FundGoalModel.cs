using Models.AccountingPeriods;
using Models.Funds;

namespace Models.FundGoals;

/// <summary>
/// Model representing a Fund Goal for an Accounting Period.
/// </summary>
public sealed class FundGoalModel
{
    /// <summary>
    /// Gets the Fund Goal ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the Fund associated with the fundGoal.
    /// </summary>
    public required FundModel Fund { get; init; }

    /// <summary>
    /// Gets the associated Accounting Period, or null for an onboarded fundGoal.
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