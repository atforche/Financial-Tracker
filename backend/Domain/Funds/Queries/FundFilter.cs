namespace Domain.Funds.Queries;

/// <summary>
/// Criteria used to filter Funds.
/// </summary>
public sealed record FundFilter(string? NameSearch, IReadOnlyCollection<string> Names);
