using Domain.AccountingPeriods;
using Models.AccountingPeriods;
using Models.Funds;
using Rest.Funds;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Period Fund Balance Histories to Accounting Period Fund Models
/// </summary>
public sealed class AccountingPeriodFundConverter(FundConverter fundConverter)
{
    /// <summary>
    /// Converts the provided Accounting Period Fund Balance History to an Accounting Period Fund Model
    /// </summary>
    public AccountingPeriodFundModel ToModel(AccountingPeriodFundBalanceHistory fundAccountingPeriodBalanceHistory)
    {
        FundModel fundModel = fundConverter.ToModel(fundAccountingPeriodBalanceHistory.Fund);
        return new()
        {
            Id = fundModel.Id,
            Description = fundModel.Description,
            Name = fundModel.Name,
            CurrentBalance = fundModel.CurrentBalance,
            OpeningBalance = fundAccountingPeriodBalanceHistory.OpeningBalance,
            AmountAssigned = fundAccountingPeriodBalanceHistory.AmountAssigned,
            AmountSpent = fundAccountingPeriodBalanceHistory.AmountSpent,
            ClosingBalance = fundAccountingPeriodBalanceHistory.ClosingBalance
        };
    }
}