using System.Diagnostics.CodeAnalysis;
using Data.Accounts;
using Microsoft.AspNetCore.Mvc;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Sort Orders to Account Sort Order Models
/// </summary>
internal sealed class AccountSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Account Sort Order Model to an Account Sort Order
    /// </summary>
    public static bool TryToData(
        AccountSortOrderModel accountSortOrderModel,
        [NotNullWhen(true)] out AccountSortOrder? accountSortOrder,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        accountSortOrder = accountSortOrderModel switch
        {
            AccountSortOrderModel.Name => AccountSortOrder.Name,
            AccountSortOrderModel.NameDescending => AccountSortOrder.NameDescending,
            AccountSortOrderModel.Type => AccountSortOrder.Type,
            AccountSortOrderModel.TypeDescending => AccountSortOrder.TypeDescending,
            AccountSortOrderModel.PostedBalance => AccountSortOrder.PostedBalance,
            AccountSortOrderModel.PostedBalanceDescending => AccountSortOrder.PostedBalanceDescending,
            AccountSortOrderModel.AvailableToSpend => AccountSortOrder.AvailableToSpend,
            AccountSortOrderModel.AvailableToSpendDescending => AccountSortOrder.AvailableToSpendDescending,
            _ => null
        };
        if (accountSortOrder == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Account Sort Order: {accountSortOrderModel}", []));
            return false;
        }
        return true;
    }
}