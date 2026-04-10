using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Balances to Account Balance Models
/// </summary>
public sealed class AccountBalanceMapper
{
    /// <summary>
    /// Maps the provided Account Balance to an Account Balance Model
    /// </summary>
    public AccountBalanceModel ToModel(AccountBalance accountBalance) => new()
    {
        AccountId = accountBalance.Account.Id.Value,
        PostedBalance = accountBalance.PostedBalance,
        AvailableToSpend = accountBalance.AvailableToSpend,
        PendingDebitAmount = accountBalance.PendingDebitAmount,
        PendingCreditAmount = accountBalance.PendingCreditAmount,
    };
}