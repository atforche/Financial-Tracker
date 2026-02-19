using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Balances to Account Balance Models
/// </summary>
public sealed class AccountBalanceMapper(FundAmountMapper fundAmountMapper)
{
    /// <summary>
    /// Maps the provided Account Balance to an Account Balance Model
    /// </summary>
    public AccountBalanceModel ToModel(AccountBalance accountBalance) => new()
    {
        AccountId = accountBalance.AccountId.Value,
        PostedBalance = accountBalance.PostedBalance,
        FundBalances = accountBalance.FundBalances.Select(fundAmountMapper.ToModel).ToList(),
        PendingDebitAmount = accountBalance.PendingDebits.Sum(fundAmount => fundAmount.Amount),
        PendingDebits = accountBalance.PendingDebits.Select(fundAmountMapper.ToModel).ToList(),
        PendingCreditAmount = accountBalance.PendingCredits.Sum(fundAmount => fundAmount.Amount),
        PendingCredits = accountBalance.PendingCredits.Select(fundAmountMapper.ToModel).ToList(),
    };
}