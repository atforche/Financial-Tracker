namespace Tests.Funds;

/// <summary>
/// Fund balance values exposed by the application.
/// </summary>
internal sealed record FundBalanceSnapshot(decimal Posted, decimal IncludingPending);