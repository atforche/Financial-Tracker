using Domain.Accounts;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Class that handles retrieving summary balances for Accounts.
/// </summary>
public class AccountSummaryGetter(IAccountRepository accountRepository, AccountConverter accountConverter)
{
    /// <summary>
    /// Gets summary balances for all Accounts.
    /// </summary>
    public AccountSummaryModel Get() => Get(accountRepository.GetAll().ToList());

    /// <summary>
    /// Gets summary balances for the provided Accounts.
    /// </summary>
    public AccountSummaryModel Get(IReadOnlyCollection<Account> accounts)
    {
        decimal totalBalance = 0;
        decimal totalTrackedBalance = 0;
        decimal totalUntrackedBalance = 0;
        Dictionary<AccountType, decimal> balancesByAccountType = [];

        foreach (Account account in accounts)
        {
            AccountModel accountModel = accountConverter.ToModel(account);
            decimal postedBalance = account.Type.IsDebt() ? -accountModel.CurrentBalance.PostedBalance : accountModel.CurrentBalance.PostedBalance;
            totalBalance += postedBalance;
            if (account.Type.IsTracked())
            {
                totalTrackedBalance += postedBalance;
            }
            else
            {
                totalUntrackedBalance += postedBalance;
            }
            balancesByAccountType[account.Type] = balancesByAccountType.GetValueOrDefault(account.Type) + postedBalance;
        }

        return new AccountSummaryModel
        {
            TotalBalance = totalBalance,
            TotalTrackedBalance = totalTrackedBalance,
            TotalUntrackedBalance = totalUntrackedBalance,
            BalanceByAccountType = balancesByAccountType
                .OrderBy(balanceByType => balanceByType.Key)
                .Select(balanceByType => new AccountTypeBalanceModel
                {
                    AccountType = AccountTypeConverter.ToModel(balanceByType.Key),
                    TotalBalance = balanceByType.Value,
                })
                .ToList(),
        };
    }
}