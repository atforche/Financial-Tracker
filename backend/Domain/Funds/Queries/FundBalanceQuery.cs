namespace Domain.Funds.Queries;

/// <summary>
/// Query used to retrieve Funds and their current balances.
/// </summary>
public sealed record FundBalanceQuery(FundFilter Filter, int Offset, int? Limit);
