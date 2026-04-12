using Domain.AccountingPeriods;
using Domain.Funds;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Accounting Period Balance Histories to Accounting Period Fund Models
/// </summary>
public sealed class AccountingPeriodFundMapper(
    FundBalanceMapper fundBalanceMapper,
    IFundGoalRepository fundGoalRepository)
{
    /// <summary>
    /// Maps the provided Fund Accounting Period Balance History to an Accounting Period Fund Model
    /// </summary>
    public AccountingPeriodFundModel ToModel(FundAccountingPeriodBalanceHistory fundAccountingPeriodBalanceHistory)
    {
        FundGoal? fundGoal = fundGoalRepository.GetByFundAndAccountingPeriod(
            fundAccountingPeriodBalanceHistory.Fund.Id,
            fundAccountingPeriodBalanceHistory.AccountingPeriod.Id);
        return new AccountingPeriodFundModel
        {
            Id = fundAccountingPeriodBalanceHistory.Fund.Id.Value,
            Name = fundAccountingPeriodBalanceHistory.Fund.Name,
            Description = fundAccountingPeriodBalanceHistory.Fund.Description,
            IsSystemFund = fundAccountingPeriodBalanceHistory.Fund.IsSystemFund,
            OpeningBalance = fundBalanceMapper.ToModel(fundAccountingPeriodBalanceHistory.GetOpeningFundBalance()),
            AmountAssigned = fundAccountingPeriodBalanceHistory.AmountAssigned,
            AmountSpent = fundAccountingPeriodBalanceHistory.AmountSpent,
            ClosingBalance = fundBalanceMapper.ToModel(fundAccountingPeriodBalanceHistory.GetClosingFundBalance()),
            GoalType = fundGoal == null ? null : FundGoalTypeMapper.ToModel(fundGoal.GoalType),
            GoalAmount = fundGoal?.GoalAmount,
            IsGoalMet = fundGoal?.IsGoalMet
        };
    }
}