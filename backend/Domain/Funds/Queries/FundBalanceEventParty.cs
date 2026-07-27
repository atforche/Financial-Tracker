namespace Domain.Funds.Queries;

/// <summary>
/// A named source or destination associated with a Fund balance event.
/// </summary>
public sealed record FundBalanceEventParty(string DisplayName, decimal? Amount);