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
    public AccountingPeriod GetById(AccountingPeriodId id) => databaseContext.AccountingPeriods
        .Single(accountingPeriod => accountingPeriod.Id == id);

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
    public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => GetMany(new GetAccountingPeriodsRequest { IsOpen = true });

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => databaseContext.Remove(accountingPeriod);

    #endregion

    /// <summary>
    /// Gets the Accounting Periods that match the specified criteria
    /// </summary>
    public IReadOnlyCollection<AccountingPeriod> GetMany(GetAccountingPeriodsRequest request)
    {
        List<AccountingPeriodSortModel> filteredAccountingPeriods = new AccountingPeriodFilterer(databaseContext).Get(request);
        filteredAccountingPeriods.Sort(new AccountingPeriodComparer(request.SortBy));
        return GetPagedAccountingPeriods(filteredAccountingPeriods.Select(model => model.AccountingPeriod).ToList(), request.Limit, request.Offset);
    }

    /// <summary>
    /// Attempts to get the Accounting Period with the specified ID.
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod)
    {
        accountingPeriod = databaseContext.AccountingPeriods.FirstOrDefault(accountingPeriod => ((Guid)(object)accountingPeriod.Id) == id);
        return accountingPeriod != null;
    }

    /// <summary>
    /// Gets the paged collection of Accounting Periods based on the provided request
    /// </summary>
    private static List<AccountingPeriod> GetPagedAccountingPeriods(List<AccountingPeriod> sortedAccountingPeriods, int? limit, int? offset)
    {
        if (offset != null)
        {
            sortedAccountingPeriods = sortedAccountingPeriods.Skip(offset.Value).ToList();
        }
        if (limit != null)
        {
            sortedAccountingPeriods = sortedAccountingPeriods.Take(limit.Value).ToList();
        }
        return sortedAccountingPeriods;
    }
}