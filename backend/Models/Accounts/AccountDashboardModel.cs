namespace Models.Accounts;

/// <summary>
/// Model representing the Account dashboard response.
/// </summary>
public class AccountDashboardModel
{
    /// <summary>
    /// Time mode used to build the dashboard response.
    /// </summary>
    public required AccountDashboardModeModel Mode { get; init; }

    /// <summary>
    /// Matching Accounts for the requested dashboard page.
    /// </summary>
    public required CollectionModel<AccountDashboardAccountModel> Accounts { get; init; }

    /// <summary>
    /// Matching balance events for the requested dashboard page.
    /// </summary>
    public required CollectionModel<AccountDashboardBalanceEventModel> BalanceEvents { get; init; }

    /// <summary>
    /// Available Account Names for the current dashboard scope before account-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Total income over the requested range.
    /// </summary>
    public required decimal TotalIncome { get; init; }

    /// <summary>
    /// Total spending over the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }

    /// <summary>
    /// Summary balances for each Accounting Period in the requested range.
    /// </summary>
    public IReadOnlyCollection<AccountDashboardPeriodSummaryModel>? AccountingPeriods { get; init; }

    /// <summary>
    /// Summary balances for each date in the requested range.
    /// </summary>
    public IReadOnlyCollection<AccountDashboardDateSummaryModel>? Dates { get; init; }
}