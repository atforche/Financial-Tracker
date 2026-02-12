using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Funds to Fund Models
/// </summary>
public sealed class FundMapper(FundBalanceService fundBalanceService, FundBalanceMapper fundBalanceMapper, IFundRepository fundRepository)
{
    /// <summary>
    /// Maps the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description,
        CurrentBalance = fundBalanceMapper.ToModel(fundBalanceService.GetCurrentBalance(fund.Id))
    };

    /// <summary>
    /// Attempts to map the provided ID to a Fund
    /// </summary>
    public bool TryToDomain(
        Guid fundId,
        [NotNullWhen(true)] out Fund? fund,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!fundRepository.TryGetById(fundId, out fund))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Fund with ID {fundId} was not found.", []));
            return false;
        }
        return true;
    }
}