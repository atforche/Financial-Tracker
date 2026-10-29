namespace Domain.AccountingPeriods;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountingPeriodBalanceHistory"/>
/// </summary>
public interface IAccountingPeriodBalanceHistoryRepository
{
    /// <summary>
    /// Gets the Accounting Period Balance History for the specified Accounting Period ID
    /// </summary>
    AccountingPeriodBalanceHistory GetForAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Gets the Accounting Period Balance Histories for the specified Accounting Period IDs
    /// </summary>
    IReadOnlyCollection<AccountingPeriodBalanceHistory> GetForAccountingPeriods(IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds);

    /// <summary>
    /// Adds the provided Accounting Period Balance History to the repository
    /// </summary>
    void Add(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory);

    /// <summary>
    /// Deletes the provided Accounting Period Balance History from the repository
    /// </summary>
    void Delete(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory);
}
