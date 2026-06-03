namespace Models.Funds;

/// <summary>
/// Model representing the Fund dashboard response.
/// </summary>
public class FundDashboardModel
{
    /// <summary>
    /// Time mode used to build the dashboard response.
    /// </summary>
    public required FundDashboardModeModel Mode { get; init; }

    /// <summary>
    /// Matching Funds for the requested dashboard page.
    /// </summary>
    public required CollectionModel<FundDashboardFundModel> Funds { get; init; }

    /// <summary>
    /// Matching balance events for the requested dashboard page.
    /// </summary>
    public required CollectionModel<FundDashboardBalanceEventModel> BalanceEvents { get; init; }

    /// <summary>
    /// Available Fund Names for the current dashboard scope before fund-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableFundNames { get; init; }

    /// <summary>
    /// Summary balances for each Accounting Period in the requested range.
    /// </summary>
    public IReadOnlyCollection<FundDashboardPeriodSummaryModel>? AccountingPeriods { get; init; }

    /// <summary>
    /// Summary balances for each date in the requested range.
    /// </summary>
    public IReadOnlyCollection<FundDashboardDateSummaryModel>? Dates { get; init; }

    /// <summary>
    /// Total amount assigned to funds from income transactions in the requested range.
    /// </summary>
    public required decimal TotalAmountAssigned { get; init; }

    /// <summary>
    /// Total amount spent from funds via spending transactions in the requested range.
    /// </summary>
    public required decimal TotalAmountSpent { get; init; }
}