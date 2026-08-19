namespace Tests.AccountingPeriods;

/// <summary>
/// Stable reference to an accounting period created through the test context.
/// </summary>
internal sealed record AccountingPeriodHandle(Guid Id, string Name);
