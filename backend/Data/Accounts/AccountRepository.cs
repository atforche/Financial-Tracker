using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;

namespace Data.Accounts;

/// <summary>
/// Repository that allows Accounts to be persisted to the database
/// </summary>
public class AccountRepository(DatabaseContext databaseContext) : IAccountRepository
{
    #region IAccountRepository

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
        List<AccountSortModel> filteredAccounts = new AccountFilterer(databaseContext).Get(request);
        filteredAccounts.Sort(new AccountComparer(request.SortBy));
        return new PaginatedCollection<Account>
        {
            Items = GetPagedAccounts(filteredAccounts.Select(model => model.Account).ToList(), request.Limit, request.Offset),
            TotalCount = filteredAccounts.Count,
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