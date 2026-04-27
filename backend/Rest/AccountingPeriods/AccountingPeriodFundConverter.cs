using Domain.AccountingPeriods;
using Domain.Funds;
using Models.AccountingPeriods;
using Rest.Funds;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Period Fund Balance Histories to Accounting Period Fund Models
/// </summary>
public sealed class AccountingPeriodFundConverter(FundGoalConverter fundGoalConverter, IFundGoalRepository fundGoalRepository)
{
    /// <summary>
    /// Converts the provided Accounting Period Fund Balance History to an Accounting Period Fund Model
    /// </summary>
    public AccountingPeriodFundModel ToModel(AccountingPeriodFundBalanceHistory fundAccountingPeriodBalanceHistory)
    {
        FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(
            fundAccountingPeriodBalanceHistory.Fund.Id,
            fundAccountingPeriodBalanceHistory.AccountingPeriod.Id);
        return new AccountingPeriodFundModel
        {
            Id = fundAccountingPeriodBalanceHistory.Fund.Id.Value,
            Name = fundAccountingPeriodBalanceHistory.Fund.Name,
            Description = fundAccountingPeriodBalanceHistory.Fund.Description,
            OpeningBalance = fundAccountingPeriodBalanceHistory.OpeningBalance,
            AmountAssigned = fundAccountingPeriodBalanceHistory.AmountAssigned,
            AmountSpent = fundAccountingPeriodBalanceHistory.AmountSpent,
            ClosingBalance = fundAccountingPeriodBalanceHistory.ClosingBalance,
            Goal = fundGoal != null ? fundGoalConverter.ToModel(fundGoal) : null
        };
    }
}