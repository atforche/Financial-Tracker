namespace Domain.AccountingPeriods;

/// <summary>
/// A posted balance at the end of one calendar day in an Accounting Period.
/// </summary>
public sealed record PeriodBalanceDate(DateOnly Date, decimal Balance);
