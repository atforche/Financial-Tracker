using System.Diagnostics.CodeAnalysis;
using Data.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Period Sort Orders to Accounting Period Sort Order Models
/// </summary>
internal sealed class AccountingPeriodSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Accounting Period Sort Order Model to an Accounting Period Sort Order
    /// </summary>
    public static bool TryToData(
        AccountingPeriodSortOrderModel accountingPeriodSortOrderModel,
        [NotNullWhen(true)] out AccountingPeriodSortOrder? accountingPeriodSortOrder)
    {
        accountingPeriodSortOrder = accountingPeriodSortOrderModel switch
        {
            AccountingPeriodSortOrderModel.Date => AccountingPeriodSortOrder.Date,
            AccountingPeriodSortOrderModel.DateDescending => AccountingPeriodSortOrder.DateDescending,
            AccountingPeriodSortOrderModel.IsOpen => AccountingPeriodSortOrder.IsOpen,
            AccountingPeriodSortOrderModel.IsOpenDescending => AccountingPeriodSortOrder.IsOpenDescending,
            _ => null
        };
        return accountingPeriodSortOrder != null;
    }
}