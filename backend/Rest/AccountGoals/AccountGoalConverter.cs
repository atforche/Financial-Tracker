using Domain.AccountGoals;
using Models.AccountGoals;
using Rest.AccountingPeriods;
using Rest.Accounts;

namespace Rest.AccountGoals;

/// <summary>
/// Converts between Account Goal API models and Domain types.
/// </summary>
public sealed class AccountGoalConverter(AccountConverter accountConverter)
{
    /// <summary>
    /// Converts an Account Goal to an API model.
    /// </summary>
    public AccountGoalModel ToModel(AccountGoal accountGoal) => new()
    {
        Id = accountGoal.Id.Value,
        Account = accountConverter.ToModel(accountGoal.Account),
        AccountingPeriod = accountGoal.AccountingPeriod == null
            ? null
            : AccountingPeriodConverter.ToModel(accountGoal.AccountingPeriod),
        MinimumEndingBalance = accountGoal.MinimumEndingBalance,
        MaximumEndingBalance = accountGoal.MaximumEndingBalance,
    };

    /// <summary>
    /// Converts Account Goal progress to an API model.
    /// </summary>
    public AccountGoalProgressModel ToModel(AccountGoalProgress progress) => new()
    {
        IsSatisfied = progress.IsSatisfied,
        EndingBalance = new AccountGoalEndingBalanceProgressModel
        {
            CurrentBalance = progress.EndingBalance.CurrentBalance,
            MinimumBalance = progress.EndingBalance.MinimumBalance,
            MaximumBalance = progress.EndingBalance.MaximumBalance,
            AmountBelowMinimum = progress.EndingBalance.AmountBelowMinimum,
            AmountAboveMaximum = progress.EndingBalance.AmountAboveMaximum,
            Status = (AccountGoalEndingBalanceStatusModel)progress.EndingBalance.Status,
        },
    };
}
