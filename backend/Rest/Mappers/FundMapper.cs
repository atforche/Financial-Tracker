using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Funds to Fund Models
/// </summary>
public sealed class FundMapper(FundBalanceService fundBalanceService, IFundRepository fundRepository)
{
    /// <summary>
    /// Maps the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund)
    {
        FundBalance currentBalance = fundBalanceService.GetCurrentBalance(fund.Id);
        return new FundModel
        {
            Id = fund.Id.Value,
            Name = fund.Name,
            Description = fund.Description,
            CurrentBalance = currentBalance.Balance,
            PendingDebitAmount = currentBalance.PendingDebits.Sum(accountAmount => accountAmount.Amount),
            PendingCreditAmount = currentBalance.PendingCredits.Sum(accountAmount => accountAmount.Amount)
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to a Fund
    /// </summary>
    public bool TryToDomain(
        Guid fundId,
        [NotNullWhen(true)] out Fund? fund,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!fundRepository.TryFindById(fundId, out fund))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Fund with ID {fundId} was not found.", []));
            return false;
        }
        return true;
    }
}