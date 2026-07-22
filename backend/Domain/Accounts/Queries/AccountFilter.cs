namespace Domain.Accounts.Queries;

/// <summary>
/// Criteria used to filter Accounts.
/// </summary>
public sealed record AccountFilter(
    string? NameSearch,
    IReadOnlyCollection<string> Names,
    IReadOnlyCollection<AccountType> Types);