using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Balances to Fund Balance Models
/// </summary>
public sealed class FundBalanceMapper(IFundRepository fundRepository)
{
    /// <summary>
    /// Maps the provided Fund Balance to a Fund Balance Model
    /// </summary>
    public FundBalanceModel ToModel(FundBalance fundBalance) => new()
    {
        FundId = fundBalance.FundId.Value,
        FundName = fundRepository.GetById(fundBalance.FundId).Name,
        PostedBalance = fundBalance.PostedBalance,
        PendingDebitAmount = fundBalance.PendingDebitAmount,
        PendingCreditAmount = fundBalance.PendingCreditAmount,
    };
}