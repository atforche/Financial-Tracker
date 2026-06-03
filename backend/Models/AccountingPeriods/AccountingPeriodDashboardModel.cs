using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the Accounting Period dashboard response.
/// </summary>
public class AccountingPeriodDashboardModel
{
    /// <summary>
    /// Matching Accounting Periods for the requested dashboard page.
    /// </summary>
    public required CollectionModel<AccountingPeriodModel> AccountingPeriods { get; init; }

    /// <summary>
    /// Matching transactions for the requested dashboard page.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Total income over the requested range.
    /// </summary>
    public required decimal TotalIncome { get; init; }

    /// <summary>
    /// Total spending over the requested range.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}