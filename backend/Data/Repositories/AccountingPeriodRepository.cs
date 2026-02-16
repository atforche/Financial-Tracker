using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository(DatabaseContext databaseContext) : IAccountingPeriodRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAll(GetAllAccountingPeriodsRequest request)
    {
        IQueryable<AccountingPeriod> filteredAccountingPeriods = GetFilteredAccountingPeriods(request);
        List<AccountingPeriod> sortedAccountingPeriods = GetSortedAccountingPeriods(filteredAccountingPeriods, request.SortBy);
        return GetPagedAccountingPeriods(sortedAccountingPeriods, request.Limit, request.Offset);
    }

    /// <inheritdoc/>
    public AccountingPeriod GetById(AccountingPeriodId id) => databaseContext.AccountingPeriods
        .Single(accountingPeriod => accountingPeriod.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod)
    {
        accountingPeriod = databaseContext.AccountingPeriods.FirstOrDefault(accountingPeriod => ((Guid)(object)accountingPeriod.Id) == id);
        return accountingPeriod != null;
    }

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
        DateOnly nextMonth = currentAccountingPeriod.PeriodStartDate.AddMonths(-1);
        return databaseContext.AccountingPeriods
            .SingleOrDefault(accountingPeriod => accountingPeriod.Year == nextMonth.Year && accountingPeriod.Month == nextMonth.Month);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => databaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen)
        .OrderBy(accountingPeriod => accountingPeriod.Year)
        .ThenBy(accountingPeriod => accountingPeriod.Month)
        .ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => databaseContext.Remove(accountingPeriod);

    /// <summary>
    /// Gets the filtered collection of Accounting Periods based on the provided request
    /// </summary>
    private IQueryable<AccountingPeriod> GetFilteredAccountingPeriods(GetAllAccountingPeriodsRequest request)
    {
        IQueryable<AccountingPeriod> results = databaseContext.AccountingPeriods.AsQueryable();
        if (request.Years != null)
        {
            results = results.Where(accountingPeriod => request.Years.Contains(accountingPeriod.Year));
        }
        if (request.Months != null)
        {
            results = results.Where(accountingPeriod => request.Months.Contains(accountingPeriod.Month));
        }
        if (request.IsOpen != null)
        {
            results = results.Where(accountingPeriod => accountingPeriod.IsOpen == request.IsOpen);
        }
        return results;
    }

    /// <summary>
    /// Gets the sorted collection of Accounting Periods based on the provided request
    /// </summary>
    private static List<AccountingPeriod> GetSortedAccountingPeriods(IQueryable<AccountingPeriod> filteredAccountingPeriods, AccountingPeriodSortOrder? sortBy)
    {
        if (sortBy is null or AccountingPeriodSortOrder.Date)
        {
            return filteredAccountingPeriods.OrderBy(accountingPeriod => accountingPeriod.Year)
                .ThenBy(accountingPeriod => accountingPeriod.Month).ToList();
        }
        if (sortBy == AccountingPeriodSortOrder.DateDescending)
        {
            return filteredAccountingPeriods.OrderByDescending(accountingPeriod => accountingPeriod.Year)
                .ThenByDescending(accountingPeriod => accountingPeriod.Month).ToList();
        }
        if (sortBy == AccountingPeriodSortOrder.IsOpen)
        {
            return filteredAccountingPeriods.OrderBy(accountingPeriod => accountingPeriod.IsOpen)
                .ThenBy(accountingPeriod => accountingPeriod.Year)
                .ThenBy(accountingPeriod => accountingPeriod.Month).ToList();
        }
        return filteredAccountingPeriods.OrderByDescending(accountingPeriod => accountingPeriod.IsOpen)
            .ThenBy(accountingPeriod => accountingPeriod.Year)
            .ThenBy(accountingPeriod => accountingPeriod.Month).ToList();
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