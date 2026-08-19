namespace Tests.Accounts;

/// <summary>
/// Account balances before and after a transaction's balance event.
/// </summary>
internal sealed record AccountBalanceEventSnapshot(
    AccountBalanceSnapshot Previous,
    AccountBalanceSnapshot New,
    bool IsPosted);
