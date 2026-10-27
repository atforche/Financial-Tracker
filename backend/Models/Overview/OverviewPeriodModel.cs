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
    /// Total income expected during the Accounting Period.
    /// </summary>
    public required decimal ExpectedIncome { get; init; }

    /// <summary>
    /// Total income received during the Accounting Period.
    /// </summary>
    public required decimal ActualIncome { get; init; }

    /// <summary>
    /// Total contributions expected toward Fund Goals during the Accounting Period.
    /// </summary>
    public required decimal ExpectedGoalContributions { get; init; }

    /// <summary>
    /// Total contributions made toward Fund Goals during the Accounting Period.
    /// </summary>
    public required decimal ActualGoalContributions { get; init; }

    /// <summary>
    /// Total posted spending during the Accounting Period.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}
