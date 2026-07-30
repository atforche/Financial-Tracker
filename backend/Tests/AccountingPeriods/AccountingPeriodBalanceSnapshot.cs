namespace Tests.AccountingPeriods;

/// <summary>
/// Accounting period balance values exposed by the application.
/// </summary>
internal sealed record AccountingPeriodBalanceSnapshot(decimal Opening, decimal Closing);