using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the Accounting Period trends response.
/// </summary>
public class AccountingPeriodTrendsModel
{
    /// <summary>
    /// Matching Accounting Periods for the requested trends page.
    /// </summary>
    public required CollectionModel<AccountingPeriodModel> AccountingPeriods { get; init; }

    /// <summary>
    /// Matching transactions for the requested trends page.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Total income over the requested range.
    /// </summary>
    public required IncomeAmountModel TotalIncome { get; init; }

    /// <summary>
    /// Total spending over the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}