using System.Globalization;
using Domain.Accounts;
using Models;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Class that handles retrieving Accounts based on specified criteria
/// </summary>
public class AccountGetter(IAccountRepository accountRepository, AccountConverter accountConverter)
{
    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    public CollectionModel<AccountModel> Get(AccountQueryParameterModel request)
    {
        var results = accountRepository.GetAll().Select(accountConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(account =>
                account.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                account.Type.ToString().Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                account.CurrentBalance.PostedBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                (account.CurrentBalance.AvailableToSpend != null && account.CurrentBalance.AvailableToSpend.Value.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        if (request.Sort is null or AccountSortOrderModel.Name)
        {
            results = results.OrderBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.Type)
        {
            results = results.OrderBy(account => account.Type).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.TypeDescending)
        {
            results = results.OrderByDescending(account => account.Type).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.PostedBalance)
        {
            results = results.OrderBy(account => account.CurrentBalance.PostedBalance).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.PostedBalanceDescending)
        {
            results = results.OrderByDescending(account => account.CurrentBalance.PostedBalance).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.AvailableToSpend)
        {
            results = results.OrderBy(account => account.CurrentBalance.AvailableToSpend).ThenBy(account => account.Name).ToList();
        }
        else if (request.Sort == AccountSortOrderModel.AvailableToSpendDescending)
        {
            results = results.OrderByDescending(account => account.CurrentBalance.AvailableToSpend).ThenBy(account => account.Name).ToList();
        }
        return new CollectionModel<AccountModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}