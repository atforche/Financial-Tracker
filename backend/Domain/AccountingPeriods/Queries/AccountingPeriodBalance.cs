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
    decimal ExpectedGoalContributions,
    decimal ActualGoalContributions);
