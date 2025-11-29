using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository(DatabaseContext databaseContext) : IAccountingPeriodRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => databaseContext.AccountingPeriods
        .AsEnumerable()
        .OrderBy(entity => entity.PeriodStartDate).ToList();

    /// <inheritdoc/>
    public AccountingPeriod FindById(AccountingPeriodId id) => databaseContext.AccountingPeriods
        .Single(accountingPeriod => accountingPeriod.Id == id);

    /// <inheritdoc/>
    public bool TryFindById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod)
    {
        accountingPeriod = databaseContext.AccountingPeriods.FirstOrDefault(accountingPeriod => ((Guid)(object)accountingPeriod.Id) == id);
        return accountingPeriod != null;
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindLatestAccountingPeriod() => databaseContext.AccountingPeriods
        .OrderBy(accountingPeriod => accountingPeriod.Year)
        .ThenBy(accountingPeriod => accountingPeriod.Month)
        .LastOrDefault();

    /// <inheritdoc/>
    public AccountingPeriod? FindNextAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = FindById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(1);
        return databaseContext.AccountingPeriods
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindPreviousAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = FindById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(-1);
        return databaseContext.AccountingPeriods
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAllOpenPeriods() => databaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => databaseContext.Remove(accountingPeriod);
}