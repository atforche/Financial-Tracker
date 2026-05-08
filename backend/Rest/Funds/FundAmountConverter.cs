using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Fund Amounts to Fund Amount Models
/// </summary>
public sealed class FundAmountConverter(FundConverter fundConverter, IFundRepository fundRepository)
{
    /// <summary>
    /// Converts the provided Fund Amount to a Fund Amount Model
    /// </summary>
    public FundAmountModel ToModel(FundAmount fundAmount) => new()
    {
        FundId = fundAmount.FundId.Value,
        FundName = fundRepository.GetById(fundAmount.FundId).Name,
        Amount = fundAmount.Amount
    };

    /// <summary>
    /// Attempts to convert the provided Fund Amount Model to a Fund Amount
    /// </summary>
    public bool TryToDomain(CreateFundAmountModel fundAmountModel, [NotNullWhen(true)] out FundAmount? fundAmount)
    {
        fundAmount = null;

        if (!fundConverter.TryToDomain(fundAmountModel.FundId, out Fund? fund))
        {
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