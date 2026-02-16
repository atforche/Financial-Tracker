using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<Account> GetAll(GetAllAccountsRequest request)
    {
        IQueryable<Account> filteredAccounts = GetFilteredAccounts(request);
        List<Account> sortedAccounts = GetSortedAccounts(filteredAccounts, request.SortBy);
        return GetPagedAccounts(sortedAccounts, request.Limit, request.Offset);
    }

    /// <inheritdoc/>
    public Account GetById(AccountId id) => databaseContext.Accounts.Single(account => account.Id == id);

    /// <inheritdoc/>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
    {
        account = databaseContext.Accounts.FirstOrDefault(account => ((Guid)(object)account.Id) == id);
        return account != null;
    }

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

    /// <summary>
    /// Gets the filtered collection of Accounts based on the provided request
    /// </summary>
    private IQueryable<Account> GetFilteredAccounts(GetAllAccountsRequest request)
    {
        IQueryable<Account> results = databaseContext.Accounts.AsQueryable();
        if (request.Names != null && request.Names.Count > 0)
        {
            results = results.Where(account => request.Names.Contains(account.Name));
        }
        if (request.Types != null && request.Types.Count > 0)
        {
            results = results.Where(account => request.Types.Contains(account.Type));
        }
        return results;
    }

    /// <summary>
    /// Gets the sorted collection of Accounts based on the provided request
    /// </summary>
    private List<Account> GetSortedAccounts(IQueryable<Account> filteredAccounts, AccountSortOrder? sortBy)
    {
        if (sortBy is null or AccountSortOrder.Name)
        {
            return filteredAccounts.OrderBy(account => account.Name).ToList();
        }
        if (sortBy == AccountSortOrder.NameDescending)
        {
            return filteredAccounts.OrderByDescending(account => account.Name).ToList();
        }
        if (sortBy == AccountSortOrder.Type)
        {
            return filteredAccounts.OrderBy(account => account.Type).ThenBy(account => account.Name).ToList();
        }
        if (sortBy == AccountSortOrder.TypeDescending)
        {
            return filteredAccounts.OrderByDescending(account => account.Type).ThenByDescending(account => account.Name).ToList();
        }
        var accountsWithBalance = filteredAccounts.GroupJoin(databaseContext.AccountBalanceHistories,
            account => account.Id,
            history => history.AccountId,
            (account, histories) => new { account, currentBalance = histories.OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault() }).ToList();
        if (sortBy == AccountSortOrder.PostedBalance)
        {
            return accountsWithBalance.OrderBy(item => item.currentBalance?.ToAccountBalance().Balance ?? 0).ThenBy(item => item.account.Name).Select(item => item.account).ToList();
        }
        if (sortBy == AccountSortOrder.PostedBalanceDescending)
        {
            return accountsWithBalance.OrderByDescending(item => item.currentBalance?.ToAccountBalance().Balance ?? 0).ThenBy(item => item.account.Name).Select(item => item.account).ToList();
        }
        return sortBy == AccountSortOrder.AvailableToSpend
            ? accountsWithBalance.OrderBy(item => (item.currentBalance?.ToAccountBalance().Balance ?? 0) - (item.currentBalance?.ToAccountBalance().PendingDebitAmount ?? 0))
                .ThenBy(item => item.account.Name).Select(item => item.account).ToList()
            : accountsWithBalance.OrderByDescending(item => (item.currentBalance?.ToAccountBalance().Balance ?? 0) - (item.currentBalance?.ToAccountBalance().PendingDebitAmount ?? 0))
                .ThenBy(item => item.account.Name).Select(item => item.account).ToList();
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