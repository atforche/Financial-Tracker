namespace Tests.Accounts;

/// <summary>
/// Account balance values exposed by the application.
/// </summary>
internal sealed record AccountBalanceSnapshot(decimal Posted, decimal IncludingPending);
