using System.Diagnostics.CodeAnalysis;
using Data.Transactions;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Transaction Sort Orders to Account Transaction Sort Order Models
/// </summary>
internal sealed class AccountTransactionSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Account Transaction Sort Order Model to an Account Transaction Sort Order
    /// </summary>
    public static bool TryToData(
        AccountTransactionSortOrderModel accountTransactionSortOrderModel,
        [NotNullWhen(true)] out AccountTransactionSortOrder? accountTransactionSortOrder)
    {
        accountTransactionSortOrder = accountTransactionSortOrderModel switch
        {
            AccountTransactionSortOrderModel.Date => AccountTransactionSortOrder.Date,
            AccountTransactionSortOrderModel.DateDescending => AccountTransactionSortOrder.DateDescending,
            AccountTransactionSortOrderModel.Location => AccountTransactionSortOrder.Location,
            AccountTransactionSortOrderModel.LocationDescending => AccountTransactionSortOrder.LocationDescending,
            AccountTransactionSortOrderModel.Type => AccountTransactionSortOrder.Type,
            AccountTransactionSortOrderModel.TypeDescending => AccountTransactionSortOrder.TypeDescending,
            AccountTransactionSortOrderModel.Amount => AccountTransactionSortOrder.Amount,
            AccountTransactionSortOrderModel.AmountDescending => AccountTransactionSortOrder.AmountDescending,
            _ => null
        };
        return accountTransactionSortOrder != null;
    }
}