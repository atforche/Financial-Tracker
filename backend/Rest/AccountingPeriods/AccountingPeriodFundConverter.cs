using Domain.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Period Fund Balance Histories to Accounting Period Fund Models
/// </summary>
public sealed class AccountingPeriodFundConverter
{
    /// <summary>
    /// Converts the provided Accounting Period Fund Balance History to an Accounting Period Fund Model
    /// </summary>
    public static AccountingPeriodFundModel ToModel(AccountingPeriodFundBalanceHistory fundAccountingPeriodBalanceHistory) =>
        new()
        {
            Id = fundAccountingPeriodBalanceHistory.Fund.Id.Value,
            Name = fundAccountingPeriodBalanceHistory.Fund.Name,
            Description = fundAccountingPeriodBalanceHistory.Fund.Description,
            OpeningBalance = fundAccountingPeriodBalanceHistory.OpeningBalance,
            AmountAssigned = fundAccountingPeriodBalanceHistory.AmountAssigned,
            AmountSpent = fundAccountingPeriodBalanceHistory.AmountSpent,
            ClosingBalance = fundAccountingPeriodBalanceHistory.ClosingBalance,
        };
}