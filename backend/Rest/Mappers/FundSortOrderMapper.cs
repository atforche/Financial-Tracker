using System.Diagnostics.CodeAnalysis;
using Data.Funds;
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
    public static bool TryToData(FundSortOrderModel fundSortOrderModel, [NotNullWhen(true)] out FundSortOrder? fundSortOrder)
    {
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
        return fundSortOrder != null;
    }
}