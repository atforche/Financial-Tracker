using Domain.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Repository that allows Accounting Period Balance Histories to be persisted to the database
/// </summary>
public class AccountingPeriodBalanceHistoryRepository(DatabaseContext databaseContext) : IAccountingPeriodBalanceHistoryRepository
{
    /// <inheritdoc/>
    public AccountingPeriodBalanceHistory GetForAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.AccountingPeriodBalanceHistories
            .SingleOrDefault(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod.Id == accountingPeriodId)
        ?? databaseContext.AccountingPeriodBalanceHistories.Local
            .Single(accountingPeriodBalanceHistory => accountingPeriodBalanceHistory.AccountingPeriod.Id == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => databaseContext.Add(accountingPeriodBalanceHistory);

    /// <inheritdoc/>
    public void Delete(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => databaseContext.Remove(accountingPeriodBalanceHistory);
}