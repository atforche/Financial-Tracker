using Domain;
using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
using Models;
using Models.AccountGoals;
using Rest.AccountingPeriods;
using Rest.Accounts;

namespace Rest.AccountGoals;

/// <summary>
/// Converts between Account Goal API models and Domain types.
/// </summary>
public sealed class AccountGoalConverter(
    AccountConverter accountConverter,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Converts an Account Goal query model to a Domain query.
    /// </summary>
    public AccountGoalQuery ToDomain(AccountGoalQueryParameterModel model) => new(
        new AccountGoalFilter(
            model.Filter?.AccountIds ?? [],
            model.Filter?.AccountingPeriodIds ?? [],
            model.Filter?.IncludeOnboarded),
        model.Sort == AccountGoalSortModel.AccountDescending
            ? AccountGoalSort.AccountDescending
            : AccountGoalSort.Account,
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an Account Goal to an API model.
    /// </summary>
    public AccountGoalModel ToModel(AccountGoal accountGoal) => new()
    {
        Id = accountGoal.Id.Value,
        Account = accountConverter.ToModel(accountGoal.Account),
        AccountingPeriod = accountGoal.AccountingPeriod == null
            ? null
            : accountingPeriodConverter.ToModel(accountGoal.AccountingPeriod),
        MinimumEndingBalance = accountGoal.MinimumEndingBalance,
        MaximumEndingBalance = accountGoal.MaximumEndingBalance,
    };

    /// <summary>
    /// Converts an Account Goal page to an API collection model.
    /// </summary>
    public CollectionModel<AccountGoalModel> ToModel(QueryPage<AccountGoal> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts Account Goal progress to an API model.
    /// </summary>
    public AccountGoalProgressModel ToModel(AccountGoalProgress progress) => new()
    {
        PositiveBalance = new PositiveBalanceProgressModel
        {
            CurrentBalance = progress.PositiveBalance.CurrentBalance,
            IsSatisfied = progress.PositiveBalance.IsSatisfied,
        },
        IsSatisfied = progress.IsSatisfied,
        EndingBalance = progress.EndingBalance == null
            ? null
            : new EndingBalanceProgressModel
            {
                CurrentBalance = progress.EndingBalance.CurrentBalance,
                MinimumBalance = progress.EndingBalance.MinimumBalance,
                MaximumBalance = progress.EndingBalance.MaximumBalance,
                AmountBelowMinimum = progress.EndingBalance.AmountBelowMinimum,
                AmountAboveMaximum = progress.EndingBalance.AmountAboveMaximum,
                Status = (EndingBalanceStatusModel)progress.EndingBalance.Status,
            },
    };
}
