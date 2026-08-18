namespace Domain.Funds.Queries;

/// <summary>
/// Query used to retrieve Funds.
/// </summary>
public sealed record FundQuery(FundFilter Filter, FundSort Sort, int Offset, int? Limit);
