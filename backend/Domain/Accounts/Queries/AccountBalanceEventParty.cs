namespace Domain.Accounts.Queries;

/// <summary>
/// A named source or destination associated with an Account balance event.
/// </summary>
public sealed record AccountBalanceEventParty(string DisplayName, decimal? Amount);
