namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents an Accounting Period and its persisted balance totals.
/// </summary>
public sealed record AccountingPeriodBalance(
    AccountingPeriod AccountingPeriod,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal ActualIncome,
    decimal ActualTrackedIncome,
    decimal ActualUnplannedIncome,
    decimal ActualUnplannedTrackedIncome,
    decimal PlannedGoalContributions,
    decimal ExpectedGoalContributions,
    decimal ActualGoalContributions,
    decimal ActualUnplannedGoalContributions)
{
    /// <summary>
    /// Gets tracked income received during the Accounting Period that was planned as one of its Expected Income
    /// Sources.
    /// </summary>
    public decimal ActualPlannedTrackedIncome => ActualTrackedIncome - ActualUnplannedTrackedIncome;
}
