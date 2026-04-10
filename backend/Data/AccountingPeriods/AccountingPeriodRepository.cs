using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;

namespace Data.AccountingPeriods;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository(DatabaseContext databaseContext) : IAccountingPeriodRepository
{
    #region IAccountingPeriodRepository

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
    public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => databaseContext.AccountingPeriods
        .Where(accountingPeriod => accountingPeriod.IsOpen)
        .ToList();

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod) => databaseContext.Add(accountingPeriod);

    /// <inheritdoc/>
    public void Delete(AccountingPeriod accountingPeriod) => databaseContext.Remove(accountingPeriod);

    #endregion

    /// <summary>
    /// Gets the Accounting Periods that match the specified criteria
    /// </summary>
    public PaginatedCollection<AccountingPeriod> GetMany(GetAccountingPeriodsRequest request)
    {
        string query = """
                        select AccountingPeriods.* from AccountingPeriods
                        left join AccountingPeriodBalanceHistories on AccountingPeriods.Id = AccountingPeriodBalanceHistories.AccountingPeriodId
                        """;
        if (request.Search != null)
        {
            query += $" where Year like '%{request.Search}%' or Month like '%{request.Search}%' or Name like '%{request.Search}%'";
        }
        if (request.Sort is null or AccountingPeriodSortOrder.DateDescending)
        {
            query += $" order by Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.Date)
        {
            query += $" order by Year asc, Month asc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.IsOpen)
        {
            query += $" order by IsOpen asc, Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.IsOpenDescending)
        {
            query += $" order by IsOpen desc, Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.OpeningBalance)
        {
            query += $" order by AccountingPeriodBalanceHistories.OpeningBalance asc, Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.OpeningBalanceDescending)
        {
            query += $" order by AccountingPeriodBalanceHistories.OpeningBalance desc, Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.ClosingBalance)
        {
            query += $" order by AccountingPeriodBalanceHistories.ClosingBalance asc, Year desc, Month desc";
        }
        else if (request.Sort == AccountingPeriodSortOrder.ClosingBalanceDescending)
        {
            query += $" order by AccountingPeriodBalanceHistories.ClosingBalance desc, Year desc, Month desc";
        }

        var accountingPeriods = databaseContext.AccountingPeriods.FromSqlRaw(query).ToList();
        return new PaginatedCollection<AccountingPeriod>
        {
            Items = GetPagedAccountingPeriods(accountingPeriods, request.Limit, request.Offset),
            TotalCount = accountingPeriods.Count,
        };
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