using System.Diagnostics.CodeAnalysis;
using Data.Transactions;
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
        [NotNullWhen(true)] out FundTransactionSortOrder? fundTransactionSortOrder)
    {
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
        return fundTransactionSortOrder != null;
    }
}