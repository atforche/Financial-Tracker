namespace Models.Funds;

/// <summary>
/// Model representing the Fund trends response.
/// </summary>
public class FundTrendsModel
{
    /// <summary>
    /// Time mode used to build the trends response.
    /// </summary>
    public required FundTrendsModeModel Mode { get; init; }

    /// <summary>
    /// Matching Funds for the requested trends page.
    /// </summary>
    public required CollectionModel<FundTrendsFundModel> Funds { get; init; }

    /// <summary>
    /// Matching balance events for the requested trends page.
    /// </summary>
    public required CollectionModel<FundTrendsBalanceEventModel> BalanceEvents { get; init; }

    /// <summary>
    /// Available Fund Names for the current trends scope before fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary balances for each Accounting Period in the requested range.
    /// </summary>
    public IReadOnlyCollection<FundTrendsPeriodSummaryModel>? AccountingPeriods { get; init; }

    /// <summary>
    /// Summary balances for each date in the requested range.
    /// </summary>
    public IReadOnlyCollection<FundTrendsDateSummaryModel>? Dates { get; init; }

    /// <summary>
    /// Total amount assigned to funds from income transactions in the requested range.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Total amount spent from funds via spending transactions in the requested range.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }
}