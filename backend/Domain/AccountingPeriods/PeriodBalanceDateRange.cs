namespace Domain.AccountingPeriods;

/// <summary>
/// Balance before the first date in the series and its daily posted balances.
/// </summary>
public sealed record PeriodBalanceDateRange(decimal OpeningBalance, IReadOnlyCollection<PeriodBalanceDate> Dates);
