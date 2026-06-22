using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the current Accounting Period response.
/// </summary>
public class CurrentAccountingPeriodModel
{
    /// <summary>
    /// The latest Accounting Period in the system.
    /// </summary>
    public AccountingPeriodModel? AccountingPeriod { get; init; }

    /// <summary>
    /// Matching transactions for the current Accounting Period page.
    /// </summary>
    public required CollectionModel<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Total income for the current Accounting Period.
    /// </summary>
    public required IncomeAmountModel TotalIncome { get; init; }

    /// <summary>
    /// Total spending for the current Accounting Period.
    /// </summary>
    public required decimal TotalSpending { get; init; }
}