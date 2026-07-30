namespace Models.Funds;

/// <summary>
/// Model representing the collection of Funds within a specified date range.
/// </summary>
public class FundsInDateRangeModel
{
    /// <summary>
    /// Matching Funds with their balances for the requested date range.
    /// </summary>
    public required CollectionModel<FundWithBalanceRangeModel> Funds { get; init; }

    /// <summary>
    /// List of all fund names in the range before fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Total income over the requested range.
    /// </summary>
    public required IncomeAmountModel TotalIncome { get; init; }

    /// <summary>
    /// Total spending over the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }

    /// <summary>
    /// Summary balances for each date in the requested range.
    /// </summary>
    public required IReadOnlyCollection<FundBalanceSummaryByDateModel> Dates { get; init; }
}