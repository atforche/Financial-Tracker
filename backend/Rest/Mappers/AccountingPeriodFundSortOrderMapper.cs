using System.Diagnostics.CodeAnalysis;
using Data.Funds;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Period Fund Sort Orders to Accounting Period Fund Sort Order Models
/// </summary>
internal sealed class AccountingPeriodFundSortOrderMapper
{
    /// <summary>
    /// Attempts to map the provided Accounting Period Fund Sort Order Model to an Accounting Period Fund Sort Order
    /// </summary>
    public static bool TryToData(
        AccountingPeriodFundSortOrderModel accountingPeriodFundSortOrderModel,
        [NotNullWhen(true)] out AccountingPeriodFundSortOrder? accountingPeriodFundSortOrder)
    {
        accountingPeriodFundSortOrder = accountingPeriodFundSortOrderModel switch
        {
            AccountingPeriodFundSortOrderModel.Name => AccountingPeriodFundSortOrder.Name,
            AccountingPeriodFundSortOrderModel.NameDescending => AccountingPeriodFundSortOrder.NameDescending,
            AccountingPeriodFundSortOrderModel.Description => AccountingPeriodFundSortOrder.Description,
            AccountingPeriodFundSortOrderModel.DescriptionDescending => AccountingPeriodFundSortOrder.DescriptionDescending,
            AccountingPeriodFundSortOrderModel.OpeningBalance => AccountingPeriodFundSortOrder.OpeningBalance,
            AccountingPeriodFundSortOrderModel.OpeningBalanceDescending => AccountingPeriodFundSortOrder.OpeningBalanceDescending,
            AccountingPeriodFundSortOrderModel.ClosingBalance => AccountingPeriodFundSortOrder.ClosingBalance,
            AccountingPeriodFundSortOrderModel.ClosingBalanceDescending => AccountingPeriodFundSortOrder.ClosingBalanceDescending,
            _ => null
        };
        return accountingPeriodFundSortOrder != null;
    }
}