using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundGoals;

/// <summary>
/// Record representing a request to create a <see cref="FundGoal"/>.
/// </summary>
public sealed record CreateFundGoalRequest
{
    /// <summary>
    /// Fund associated with the Fund Goal.
    /// </summary>
    public Fund Fund { get; init; } = null!;

    /// <summary>
    /// Accounting Period associated with the Fund Goal, or null for an onboarded Fund Goal.
    /// </summary>
    public AccountingPeriod? AccountingPeriod { get; init; }

    /// <summary>
    /// Amount normally contributed during each Accounting Period.
    /// </summary>
    public decimal? PlannedMonthlyContribution { get; init; }

    /// <summary>
    /// Minimum desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum desired balance at the end of an Accounting Period.
    /// </summary>
    public decimal? MaximumEndingBalance { get; init; }
}
