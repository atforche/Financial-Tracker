using System.Diagnostics.CodeAnalysis;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Sort Orders to Fund Sort Order Models
/// </summary>
internal sealed class FundSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Fund Sort Order Model to a Fund Sort Order
    /// </summary>
    public static bool TryToDomain(
        FundSortOrderModel fundSortOrderModel,
        [NotNullWhen(true)] out FundSortOrder? fundSortOrder,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        fundSortOrder = fundSortOrderModel switch
        {
            FundSortOrderModel.Name => FundSortOrder.Name,
            FundSortOrderModel.NameDescending => FundSortOrder.NameDescending,
            FundSortOrderModel.Description => FundSortOrder.Description,
            FundSortOrderModel.DescriptionDescending => FundSortOrder.DescriptionDescending,
            FundSortOrderModel.Balance => FundSortOrder.Balance,
            FundSortOrderModel.BalanceDescending => FundSortOrder.BalanceDescending,
            _ => null
        };
        if (fundSortOrder == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Fund Sort Order Model: {fundSortOrderModel}", []));
            return false;
        }
        return true;
    }
}