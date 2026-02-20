using System.Diagnostics.CodeAnalysis;
using Data.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Transaction Sort Orders to Fund Transaction Sort Order Models
/// </summary>
internal sealed class FundTransactionSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Fund Transaction Sort Order Model to a Fund Transaction Sort Order
    /// </summary>
    public static bool TryToData(
        FundTransactionSortOrderModel fundTransactionSortOrderModel,
        [NotNullWhen(true)] out FundTransactionSortOrder? fundTransactionSortOrder,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        fundTransactionSortOrder = fundTransactionSortOrderModel switch
        {
            FundTransactionSortOrderModel.Date => FundTransactionSortOrder.Date,
            FundTransactionSortOrderModel.DateDescending => FundTransactionSortOrder.DateDescending,
            FundTransactionSortOrderModel.Location => FundTransactionSortOrder.Location,
            FundTransactionSortOrderModel.LocationDescending => FundTransactionSortOrder.LocationDescending,
            FundTransactionSortOrderModel.Type => FundTransactionSortOrder.Type,
            FundTransactionSortOrderModel.TypeDescending => FundTransactionSortOrder.TypeDescending,
            FundTransactionSortOrderModel.Amount => FundTransactionSortOrder.Amount,
            FundTransactionSortOrderModel.AmountDescending => FundTransactionSortOrder.AmountDescending,
            _ => null
        };
        if (fundTransactionSortOrder == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Fund Transaction Sort Order: {fundTransactionSortOrderModel}", []));
            return false;
        }
        return true;
    }
}