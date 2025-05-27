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
    public bool DoesAccountingPeriodWithIdExist(Guid id) => databaseContext.AccountingPeriods
        .Any(accountingPeriod => ((Guid)(object)accountingPeriod.Id) == id);

    /// <inheritdoc/>
    public AccountingPeriod FindById(AccountingPeriodId id) => databaseContext.AccountingPeriods
        .Single(accountingPeriod => accountingPeriod.Id == id);

    /// <inheritdoc/>
    public AccountingPeriod? FindByDateOrNull(DateOnly asOfDate) => databaseContext.AccountingPeriods
        .SingleOrDefault(accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindOpenPeriods() => databaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen).ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);
}