namespace Domain.Accounts.Queries;

/// <summary>
/// Criteria for querying balance events for a single Account.
/// </summary>
public sealed record AccountBalanceEventAccountQuery(
    Guid AccountId,
    DateOnly Start,
    DateOnly End,
    AccountBalanceEventSort Sort,
    int Offset,
    int? Limit);
