using Models.AccountingPeriods;

namespace Models.Overview;

/// <summary>
/// Model representing the figures for the most recent open Accounting Period.
/// </summary>
public class OverviewPeriodModel
{
    /// <summary>
    /// The most recent open Accounting Period.
    /// </summary>
    public required AccountingPeriodModel AccountingPeriod { get; init; }

    /// <summary>
    /// Tracked income expected during the Accounting Period.
    /// </summary>
    public required decimal ExpectedTrackedIncome { get; init; }

    /// <summary>
    /// Tracked income received during the Accounting Period that was planned as one of its Expected Income Sources.
    /// </summary>
    public required decimal ActualPlannedTrackedIncome { get; init; }

    /// <summary>
    /// Tracked income received during the Accounting Period that was not planned as any Expected Income Source.
    /// </summary>
    public required decimal ActualUnplannedTrackedIncome { get; init; }

    /// <summary>
    /// Total contributions expected toward Fund Goals during the Accounting Period.
    /// </summary>
    public required decimal ExpectedGoalContributions { get; init; }

    /// <summary>
    /// Total contributions made toward Fund Goals during the Accounting Period.
    /// </summary>
    public required decimal ActualGoalContributions { get; init; }

    /// <summary>
    /// Unplanned income assigned to Fund Goals during the Accounting Period, which does not count toward their
    /// contributions.
    /// </summary>
    public required decimal ActualUnplannedGoalContributions { get; init; }

    /// <summary>
    /// Total posted spending during the Accounting Period.
    /// </summary>
    public required decimal TotalSpending { get; init; }

    /// <summary>
    /// Change in net worth from the start of the Accounting Period, across every Account with debts subtracted.
    /// </summary>
    public required decimal NetWorthChange { get; init; }
}
