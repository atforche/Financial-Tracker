using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;

namespace Data.AccountingPeriods;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository(DatabaseContext databaseContext) : IAccountingPeriodRepository
{
    #region IAccountingPeriodRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAll() => databaseContext.AccountingPeriods.ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => databaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen)
        .ToList();

    /// <inheritdoc/>
    public AccountingPeriod GetById(AccountingPeriodId id) => databaseContext.AccountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Id == id)
        ?? databaseContext.AccountingPeriods.Local.Single(accountingPeriod => accountingPeriod.Id == id);

    /// <inheritdoc/>
    public AccountingPeriod? GetByYearAndMonth(int year, int month) => databaseContext.AccountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Year == year && accountingPeriod.Month == month);

    /// <inheritdoc/>
    public AccountingPeriod? GetLatestAccountingPeriod() => databaseContext.AccountingPeriods
        .OrderBy(accountingPeriod => accountingPeriod.Year)
        .ThenBy(accountingPeriod => accountingPeriod.Month)
        .LastOrDefault();

    /// <inheritdoc/>
    public AccountingPeriod? GetNextAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = GetById(id);
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(1);
        return databaseContext.AccountingPeriods
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public AccountingPeriod? GetPreviousAccountingPeriod(AccountingPeriodId id)
    {
        AccountingPeriod currentAccountingPeriod = GetById(id);
        DateOnly previousMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(-1);
        return databaseContext.AccountingPeriods
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == previousMonth.Year && accountingPeriod.Month == previousMonth.Month);
    }

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => databaseContext.Remove(accountingPeriod);

    #endregion

    /// <summary>
    /// Attempts to get the Accounting Period with the specified ID.
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod)
    {
        accountingPeriod = databaseContext.AccountingPeriods.FirstOrDefault(accountingPeriod => ((Guid)(object)accountingPeriod.Id) == id);
        return accountingPeriod != null;
    }
}