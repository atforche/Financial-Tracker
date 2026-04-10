using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Data.Accounts;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    #region IAccountRepository

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAll() => databaseContext.Accounts.ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAllAccountsAddedInPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.Accounts.Where(account => account.AddAccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public Account GetById(AccountId id) => databaseContext.Accounts.SingleOrDefault(account => account.Id == id)
        ?? databaseContext.Accounts.Local.Single(account => account.Id == id);

    /// <inheritdoc/>
    public bool TryGetByName(string name, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => account.Name == name);
        return account != null;
    }

    /// <inheritdoc/>
    public void Add(Account account) => databaseContext.Add(account);

    /// <inheritdoc/>
    public void Delete(Account account) => databaseContext.Remove(account);

    #endregion

    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    public PaginatedCollection<Account> GetMany(GetAccountsRequest request)
    {
        string query = """
                        select Accounts.* from Accounts 
                        left join AccountBalanceHistories on Accounts.Id = AccountBalanceHistories.AccountId 
                            and AccountBalanceHistories.Date = (select max(Date) from AccountBalanceHistories where AccountBalanceHistories.AccountId = Accounts.Id)
                            and AccountBalanceHistories.Sequence = (select max(Sequence) from AccountBalanceHistories where AccountBalanceHistories.AccountId = Accounts.Id and AccountBalanceHistories.Date = (select max(Date) from AccountBalanceHistories where AccountBalanceHistories.AccountId = Accounts.Id))
                        """;
        if (request.Search != null)
        {
            query += $" where Accounts.Name like '%{request.Search}%' or Accounts.Type like '%{request.Search}%' or AccountBalanceHistories.PostedBalance like '%{request.Search}%' or AccountBalanceHistories.AvailableToSpend like '%{request.Search}%'";
        }
        if (request.Sort is null or AccountSortOrder.Name)
        {
            query += $" order by Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.NameDescending)
        {
            query += $" order by Accounts.Name desc";
        }
        else if (request.Sort == AccountSortOrder.Type)
        {
            query += $" order by Accounts.Type asc, Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.TypeDescending)
        {
            query += $" order by Accounts.Type desc, Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.PostedBalance)
        {
            query += $" order by AccountBalanceHistories.PostedBalance asc, Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.PostedBalanceDescending)
        {
            query += $" order by AccountBalanceHistories.PostedBalance desc, Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.AvailableToSpend)
        {
            query += $" order by AccountBalanceHistories.AvailableToSpend asc, Accounts.Name asc";
        }
        else if (request.Sort == AccountSortOrder.AvailableToSpendDescending)
        {
            query += $" order by AccountBalanceHistories.AvailableToSpend desc, Accounts.Name asc";
        }

        var accounts = databaseContext.Accounts.FromSqlRaw(query).ToList();
        return new PaginatedCollection<Account>
        {
            Items = GetPagedAccounts(accounts, request.Limit, request.Offset),
            TotalCount = accounts.Count,
        };
    }

    /// <summary>
    /// Attempts to get the Account with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => ((Guid)(object)account.Id) == id);
        return account != null;
    }

    /// <summary>
    /// Gets the paged collection of Accounts based on the provided request
    /// </summary>
    private static List<Account> GetPagedAccounts(List<Account> sortedAccounts, int? limit, int? offset)
    {
        if (offset != null)
        {
            sortedAccounts = sortedAccounts.Skip(offset.Value).ToList();
        }
        if (limit != null)
        {
            sortedAccounts = sortedAccounts.Take(limit.Value).ToList();
        }
        return sortedAccounts;
    }
}