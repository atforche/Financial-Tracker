namespace Models.Accounts;

/// <summary>
/// Model representing the Account trends response.
/// </summary>
public class AccountTrendsModel
{
    /// <summary>
    /// Time mode used to build the trends response.
    /// </summary>
    public required AccountTrendsModeModel Mode { get; init; }

    /// <summary>
    /// Matching Accounts for the requested trends page.
    /// </summary>
    public required CollectionModel<AccountTrendsAccountModel> Accounts { get; init; }

    /// <summary>
    /// Matching balance events for the requested trends page.
    /// </summary>
    public required CollectionModel<AccountTrendsBalanceEventModel> BalanceEvents { get; init; }

    /// <summary>
    /// Available Account Names for the current trends scope before account-name filtering.
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
    public IReadOnlyCollection<AccountTrendsPeriodSummaryModel>? AccountingPeriods { get; init; }

    /// <summary>
    /// Summary balances for each date in the requested range.
    /// </summary>
    public IReadOnlyCollection<AccountTrendsDateSummaryModel>? Dates { get; init; }
}