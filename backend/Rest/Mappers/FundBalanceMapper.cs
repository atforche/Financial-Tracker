using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Balances to Fund Balance Models
/// </summary>
public sealed class FundBalanceMapper(AccountAmountMapper accountAmountMapper)
{
    /// <summary>
    /// Maps the provided Fund Balance to a Fund Balance Model
    /// </summary>
    public FundBalanceModel ToModel(FundBalance fundBalance) => new()
    {
        FundId = fundBalance.FundId.Value,
        PostedBalance = fundBalance.PostedBalance,
        AccountBalances = fundBalance.AccountBalances.Select(accountAmountMapper.ToModel).ToList(),
        PendingDebitAmount = fundBalance.PendingDebits.Sum(accountAmount => accountAmount.Amount),
        PendingDebits = fundBalance.PendingDebits.Select(accountAmountMapper.ToModel).ToList(),
        PendingCreditAmount = fundBalance.PendingCredits.Sum(accountAmount => accountAmount.Amount),
        PendingCredits = fundBalance.PendingCredits.Select(accountAmountMapper.ToModel).ToList(),
    };
}