using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving summary balances for Funds.
/// </summary>
public class FundSummaryGetter(IFundRepository fundRepository, FundConverter fundConverter)
{
    /// <summary>
    /// Gets summary balances for all Funds.
    /// </summary>
    public FundSummaryModel Get()
    {
        decimal totalTrackedBalance = 0;
        decimal totalAssignedBalance = 0;
        decimal totalUnassignedBalance = 0;

        foreach (Fund fund in fundRepository.GetAll())
        {
            FundModel fundModel = fundConverter.ToModel(fund);
            decimal postedBalance = fundModel.CurrentBalance.PostedBalance;
            totalTrackedBalance += postedBalance;
            if (fund.Name == Fund.UnassignedFundName)
            {
                totalUnassignedBalance += postedBalance;
            }
            else
            {
                totalAssignedBalance += postedBalance;
            }
        }
        return new FundSummaryModel
        {
            TotalTrackedBalance = totalTrackedBalance,
            TotalAssignedBalance = totalAssignedBalance,
            TotalUnassignedBalance = totalUnassignedBalance,
        };
    }
}