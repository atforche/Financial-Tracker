using System.Diagnostics.CodeAnalysis;
using Data.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Period Transaction Sort Orders to Accounting Period Transaction Sort Order Models
/// </summary>
internal sealed class AccountingPeriodTransactionSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Accounting Period Transaction Sort Order Model to an Accounting Period Transaction Sort Order
    /// </summary>
    public static bool TryToData(
        AccountingPeriodTransactionSortOrderModel accountingPeriodTransactionSortOrderModel,
        [NotNullWhen(true)] out AccountingPeriodTransactionSortOrder? accountingPeriodTransactionSortOrder,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        accountingPeriodTransactionSortOrder = accountingPeriodTransactionSortOrderModel switch
        {
            AccountingPeriodTransactionSortOrderModel.Date => AccountingPeriodTransactionSortOrder.Date,
            AccountingPeriodTransactionSortOrderModel.DateDescending => AccountingPeriodTransactionSortOrder.DateDescending,
            AccountingPeriodTransactionSortOrderModel.Location => AccountingPeriodTransactionSortOrder.Location,
            AccountingPeriodTransactionSortOrderModel.LocationDescending => AccountingPeriodTransactionSortOrder.LocationDescending,
            AccountingPeriodTransactionSortOrderModel.DebitAccount => AccountingPeriodTransactionSortOrder.DebitAccount,
            AccountingPeriodTransactionSortOrderModel.DebitAccountDescending => AccountingPeriodTransactionSortOrder.DebitAccountDescending,
            AccountingPeriodTransactionSortOrderModel.CreditAccount => AccountingPeriodTransactionSortOrder.CreditAccount,
            AccountingPeriodTransactionSortOrderModel.CreditAccountDescending => AccountingPeriodTransactionSortOrder.CreditAccountDescending,
            AccountingPeriodTransactionSortOrderModel.Amount => AccountingPeriodTransactionSortOrder.Amount,
            AccountingPeriodTransactionSortOrderModel.AmountDescending => AccountingPeriodTransactionSortOrder.AmountDescending,
            _ => null
        };
        if (accountingPeriodTransactionSortOrder == null)
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Unrecognized Accounting Period Transaction Sort Order: {accountingPeriodTransactionSortOrderModel}", []));
            return false;
        }
        return true;
    }
}