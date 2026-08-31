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
    /// Gets the associated Accounting Period, or null for an onboarded Fund Goal.
    /// </summary>
    public AccountingPeriodModel? AccountingPeriod { get; init; }

    /// <summary>
    /// Gets the regular contribution.
    /// </summary>
    public decimal? RegularContribution { get; init; }

    /// <summary>
    /// Gets the minimum ending balance.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Gets the maximum ending balance.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
