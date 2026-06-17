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
    public FundSummaryModel Get() => Get(fundRepository.GetAll().ToList());

    /// <summary>
    /// Gets summary balances for the provided Funds.
    /// </summary>
    public FundSummaryModel Get(IReadOnlyCollection<Fund> funds)
    {
        decimal totalTrackedBalance = 0;
        decimal totalAssignedBalance = 0;
        decimal totalUnassignedBalance = 0;

        foreach (Fund fund in funds)
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