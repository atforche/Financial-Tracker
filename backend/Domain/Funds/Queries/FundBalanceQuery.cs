namespace Domain.Funds.Queries;

/// <summary>
/// Query used to retrieve Funds and their current balances.
/// </summary>
public sealed record FundBalanceQuery(FundFilter Filter, FundBalanceSort Sort, int Offset, int? Limit);