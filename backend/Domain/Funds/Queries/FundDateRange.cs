namespace Domain.Funds.Queries;

/// <summary>
/// Represents Fund balances and totals over a date range.
/// </summary>
public sealed record FundDateRange(
    QueryPage<FundRangeBalance> Funds,
    IReadOnlyCollection<string> AvailableFundNames,
    FundBalanceSummary? OpeningBalance,
    BalanceGranularity Granularity,
    IReadOnlyCollection<FundDateBalanceSummary> Dates);

/// <summary>
/// Represents a Fund balance summary for a date.
/// </summary>
public sealed record FundDateBalanceSummary(DateOnly Date, FundBalanceSummary Balance);
