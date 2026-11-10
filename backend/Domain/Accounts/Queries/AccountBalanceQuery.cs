namespace Domain.Accounts.Queries;

/// <summary>
/// Query used to retrieve Accounts and their current balances.
/// </summary>
public sealed record AccountBalanceQuery(AccountFilter Filter, int Offset, int? Limit);
