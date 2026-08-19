namespace Models.Accounts;

/// <summary>
/// Model representing the collection of Accounts within a specified accounting period range.
/// </summary>
public class AccountsInAccountingPeriodRangeModel
{
    /// <summary>
    /// Matching Accounts with their balances for the requested accounting period range.
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
    /// Summary balances for each accounting period in the requested range.
    /// </summary>
    public required IReadOnlyCollection<AccountBalanceSummaryByPeriodModel> AccountingPeriods { get; init; }
}
