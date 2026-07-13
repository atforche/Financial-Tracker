using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Fund Balances to Fund Balance Models
/// </summary>
public sealed class FundBalanceConverter
{
    /// <summary>
    /// Converts the provided Fund Balance to a Fund Balance Model
    /// </summary>
    public static FundBalanceModel ToModel(Fund fund, FundBalance? fundBalance)
    {
        if (fundBalance == null)
        {
            return new FundBalanceModel
            {
                FundId = fund.Id.Value,
                FundName = fund.Name,
                PostedBalance = 0,
            };
        }
        return new FundBalanceModel
        {
            FundId = fundBalance.FundId.Value,
            FundName = fund.Name,
            PostedBalance = fundBalance.PostedBalance,
        };
    }
}