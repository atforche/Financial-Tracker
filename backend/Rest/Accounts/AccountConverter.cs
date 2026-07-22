using Domain;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Models;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Converts between Account API models and Domain query types.
/// </summary>
public sealed class AccountConverter
{
    /// <summary>
    /// Converts the provided Account query model to a Domain query.
    /// </summary>
    public AccountQuery ToDomain(AccountQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountSortModel.Name => AccountSort.Name,
            AccountSortModel.NameDescending => AccountSort.NameDescending,
            AccountSortModel.Type => AccountSort.Type,
            AccountSortModel.TypeDescending => AccountSort.TypeDescending,
            _ => AccountSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account Balance query model to a Domain query.
    /// </summary>
    public AccountBalanceQuery ToDomain(AccountWithBalanceQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountWithBalanceSortModel.Name => AccountBalanceSort.Name,
            AccountWithBalanceSortModel.NameDescending => AccountBalanceSort.NameDescending,
            AccountWithBalanceSortModel.Type => AccountBalanceSort.Type,
            AccountWithBalanceSortModel.TypeDescending => AccountBalanceSort.TypeDescending,
            AccountWithBalanceSortModel.PostedBalance => AccountBalanceSort.PostedBalance,
            AccountWithBalanceSortModel.PostedBalanceDescending => AccountBalanceSort.PostedBalanceDescending,
            _ => AccountBalanceSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account to an Account model.
    /// </summary>
    public AccountModel ToModel(Account account) => new()
    {
        Id = account.Id.Value,
        Name = account.Name,
        Type = AccountTypeConverter.ToModel(account.Type),
    };

    /// <summary>
    /// Converts the provided Account page to an Account collection model.
    /// </summary>
    public CollectionModel<AccountModel> ToModel(QueryPage<Account> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Account Balance to an Account with Balance model.
    /// </summary>
    public AccountWithBalanceModel ToModel(AccountBalance balance) => new()
    {
        Id = balance.Account.Id.Value,
        Name = balance.Account.Name,
        Type = AccountTypeConverter.ToModel(balance.Account.Type),
        CurrentBalance = new AccountBalanceModel
        {
            PostedBalance = balance.PostedBalance,
            PendingDebitAmount = balance.PendingDebitAmount,
            PendingCreditAmount = balance.PendingCreditAmount,
            PendingBalance = balance.PendingBalance,
        },
    };

    /// <summary>
    /// Converts the provided Account Balance page to an Account with Balance collection model.
    /// </summary>
    public CollectionModel<AccountWithBalanceModel> ToModel(QueryPage<AccountBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Account filter model to a Domain filter.
    /// </summary>
    private static AccountFilter ToDomain(AccountFilterModel? model) => new(
        model?.NameSearch,
        model?.Names ?? [],
        model?.Types?.Select(type => (AccountType)type).ToList() ?? []);
}