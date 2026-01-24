using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Amounts to Fund Amount Models
/// </summary>
public sealed class FundAmountMapper(IFundRepository fundRepository)
{
    /// <summary>
    /// Converts the provided Fund Amount model into a Fund Amount
    /// </summary>
    /// <param name="fundAmountModel">Fund Amount model to convert</param>
    /// <param name="fundAmount">The Fund Amount corresponding to the provided Fund Amount model</param>
    /// <param name="exception">Exception encountered during mapping, if any</param>
    /// <returns>True if mapping was successful, false otherwise</returns>
    public bool TryMapToDomain(FundAmountModel fundAmountModel, [NotNullWhen(true)] out FundAmount? fundAmount, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        fundAmount = null;
        if (!fundRepository.TryFindById(fundAmountModel.FundId, out Fund? fund))
        {
            exception = new InvalidOperationException($"Fund with ID {fundAmountModel.FundId} could not be found.");
            return false;
        }
        fundAmount = new FundAmount
        {
            FundId = fund.Id,
            Amount = fundAmountModel.Amount
        };
        return true;
    }
}