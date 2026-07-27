namespace Domain.FundGoals.Queries;

/// <summary>
/// A named source or destination associated with a Fund Goal balance event.
/// </summary>
public sealed record FundGoalBalanceEventParty(string DisplayName, decimal? Amount);