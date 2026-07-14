using Models.Transactions;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Accounting Period with its associated transactions.
/// </summary>
public class AccountingPeriodWithTransactionsModel : AccountingPeriodWithBalanceModel
{
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