namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Represents a contiguous range of Accounting Periods, in date order, and its total spending.
/// </summary>
public sealed record AccountingPeriodRange(
    IReadOnlyCollection<AccountingPeriodBalance> AccountingPeriods,
    decimal TotalSpending);
