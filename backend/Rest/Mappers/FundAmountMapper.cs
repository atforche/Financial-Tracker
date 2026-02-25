using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Amounts to Fund Amount Models
/// </summary>
public sealed class FundAmountMapper(FundMapper fundMapper, IFundRepository fundRepository)
{
    /// <summary>
    /// Maps the provided Fund Amount to a Fund Amount Model
    /// </summary>
    public FundAmountModel ToModel(FundAmount fundAmount) => new()
    {
        FundId = fundAmount.FundId.Value,
        FundName = fundRepository.GetById(fundAmount.FundId).Name,
        Amount = fundAmount.Amount
    };

    /// <summary>
    /// Attempts to map the provided Fund Amount Model to a Fund Amount
    /// </summary>
    public bool TryToDomain(CreateFundAmountModel fundAmountModel, [NotNullWhen(true)] out FundAmount? fundAmount)
    {
        fundAmount = null;

        if (!fundMapper.TryToDomain(fundAmountModel.FundId, out Fund? fund))
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