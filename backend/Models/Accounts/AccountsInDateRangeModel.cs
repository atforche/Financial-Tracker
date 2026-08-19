namespace Models.Accounts;

/// <summary>
/// Model representing the collection of Accounts within a specified date range.
/// </summary>
public class AccountsInDateRangeModel
{
    /// <summary>
    /// Matching Accounts with their balances for the requested date range.
    /// </summary>
    public required CollectionModel<AccountWithBalanceRangeModel> Accounts { get; init; }

    /// <summary>
    /// List of all account names in the range before account-name filtering.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

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
    public required IReadOnlyCollection<AccountBalanceSummaryByDateModel> Dates { get; init; }
}
