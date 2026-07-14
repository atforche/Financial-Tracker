namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the Accounting Periods in a requested range.
/// </summary>
public class AccountingPeriodsInRangeModel
{
    /// <summary>
    /// Accounting Periods in the requested range.
    /// </summary>
    public required CollectionModel<AccountingPeriodWithBalanceModel> AccountingPeriods { get; init; }

    /// <summary>
    /// Total income for the requested range.
    /// </summary>
    public required IncomeAmountModel TotalIncome { get; init; }

    /// <summary>
    /// Total spending for the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}