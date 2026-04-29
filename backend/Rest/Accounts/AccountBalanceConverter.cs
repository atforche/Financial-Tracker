using Domain.Accounts;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Converter class that handles converting Account Balances to Account Balance Models
/// </summary>
public sealed class AccountBalanceConverter
{
    /// <summary>
    /// Converts the provided Account Balance to an Account Balance Model
    /// </summary>
    public static AccountBalanceModel ToModel(AccountBalance accountBalance) => new()
    {
        AccountId = accountBalance.Account.Id.Value,
        PostedBalance = accountBalance.PostedBalance,
        AvailableToSpend = accountBalance.AvailableToSpend,
        PendingDebitAmount = accountBalance.PendingDebitAmount,
        PendingCreditAmount = accountBalance.PendingCreditAmount,
    };
}