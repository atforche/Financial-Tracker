namespace Models.Funds;

/// <summary>
/// Query parameters for retrieving Funds with current balances.
/// </summary>
public class FundWithBalanceQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Filters to apply.
    /// </summary>
    public FundFilterModel? Filter { get; init; }
}
