using System.Diagnostics.CodeAnalysis;
using Data.Accounts;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Period Account Sort Orders to Accounting Period Account Sort Order Models
/// </summary>
internal sealed class AccountingPeriodAccountSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Accounting Period Account Sort Order Model to an Accounting Period Account Sort Order
    /// </summary>
    public static bool TryToData(
        AccountingPeriodAccountSortOrderModel accountingPeriodAccountSortOrderModel,
        [NotNullWhen(true)] out AccountingPeriodAccountSortOrder? accountingPeriodAccountSortOrder)
    {
        accountingPeriodAccountSortOrder = accountingPeriodAccountSortOrderModel switch
        {
            AccountingPeriodAccountSortOrderModel.Name => AccountingPeriodAccountSortOrder.Name,
            AccountingPeriodAccountSortOrderModel.NameDescending => AccountingPeriodAccountSortOrder.NameDescending,
            AccountingPeriodAccountSortOrderModel.Type => AccountingPeriodAccountSortOrder.Type,
            AccountingPeriodAccountSortOrderModel.TypeDescending => AccountingPeriodAccountSortOrder.TypeDescending,
            AccountingPeriodAccountSortOrderModel.OpeningBalance => AccountingPeriodAccountSortOrder.OpeningBalance,
            AccountingPeriodAccountSortOrderModel.OpeningBalanceDescending => AccountingPeriodAccountSortOrder.OpeningBalanceDescending,
            AccountingPeriodAccountSortOrderModel.ClosingBalance => AccountingPeriodAccountSortOrder.ClosingBalance,
            AccountingPeriodAccountSortOrderModel.ClosingBalanceDescending => AccountingPeriodAccountSortOrder.ClosingBalanceDescending,
            _ => null
        };
        return accountingPeriodAccountSortOrder != null;
    }
}