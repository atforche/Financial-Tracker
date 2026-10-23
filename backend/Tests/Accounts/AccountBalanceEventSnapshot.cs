namespace Tests.Accounts;

/// <summary>
/// Posted account balances before and after a transaction's balance event, which are null until it posts.
/// </summary>
internal sealed record AccountBalanceEventSnapshot(
    decimal? Previous,
    decimal? New,
    bool IsPosted);
