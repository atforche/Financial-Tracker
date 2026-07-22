namespace Domain.Accounts.Queries;

/// <summary>
/// Query used to retrieve Accounts.
/// </summary>
public sealed record AccountQuery(AccountFilter Filter, AccountSort Sort, int Offset, int? Limit);