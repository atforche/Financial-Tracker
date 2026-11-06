using Domain;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Models;
using Models.AccountingPeriods;
using Rest.AccountGoals;
using Rest.Accounts;
using Rest.FundGoals;
using Rest.Funds;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converts between Accounting Period API models and Domain query types.
/// </summary>
public sealed class AccountingPeriodQueryConverter(
    AccountingPeriodConverter accountingPeriodConverter,
    AccountConverter accountConverter,
    FundConverter fundConverter,
    AccountGoalConverter accountGoalConverter,
    FundGoalConverter fundGoalConverter)
{
    /// <summary>
    /// Converts the provided Accounting Period query model to a Domain query.
    /// </summary>
    public AccountingPeriodQuery ToDomain(AccountingPeriodQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountingPeriodSortModel.Date => AccountingPeriodSort.Date,
            AccountingPeriodSortModel.DateDescending => AccountingPeriodSort.DateDescending,
            AccountingPeriodSortModel.IsOpen => AccountingPeriodSort.IsOpen,
            AccountingPeriodSortModel.IsOpenDescending => AccountingPeriodSort.IsOpenDescending,
            _ => AccountingPeriodSort.DateDescending,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Accounting Period Balance query model to a Domain query.
    /// </summary>
    public AccountingPeriodBalanceQuery ToDomain(AccountingPeriodWithBalanceQueryParameterModel model) => new(
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Accounting Period range query model to a Domain query.
    /// </summary>
    public AccountingPeriodRangeQuery ToDomain(AccountingPeriodsInRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End);

    /// <summary>
    /// Converts the provided Accounting Period to an API model.
    /// </summary>
    public AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => accountingPeriodConverter.ToModel(accountingPeriod);

    /// <summary>
    /// Converts the provided earliest and latest Accounting Periods to an API model.
    /// </summary>
    public AccountingPeriodBoundsModel ToModel((AccountingPeriod? First, AccountingPeriod? Latest) bounds) => new()
    {
        First = bounds.First == null ? null : ToModel(bounds.First),
        Latest = bounds.Latest == null ? null : ToModel(bounds.Latest),
    };

    /// <summary>
    /// Converts the provided Accounting Period page to a collection model.
    /// </summary>
    public CollectionModel<AccountingPeriodModel> ToModel(QueryPage<AccountingPeriod> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Accounting Period Balance to an API model.
    /// </summary>
    public AccountingPeriodWithBalanceModel ToModel(AccountingPeriodBalance balance) => new()
    {
        Id = balance.AccountingPeriod.Id.Value,
        Name = balance.AccountingPeriod.Name,
        Year = balance.AccountingPeriod.Year,
        Month = balance.AccountingPeriod.Month,
        IsOpen = balance.AccountingPeriod.IsOpen,
        OpeningBalance = balance.OpeningBalance,
        ClosingBalance = balance.ClosingBalance,
        ExpectedIncomeSources = balance.AccountingPeriod.ExpectedIncomeSources
            .Select(source => accountingPeriodConverter.ToModel(source, balance.ReceivedIncome.Get(source.Id)))
            .ToList(),
        ExpectedIncome = ToExpectedIncomeAmountModel(balance.AccountingPeriod.ExpectedIncomeSources),
        ActualIncome = new IncomeAmountModel
        {
            Total = balance.ActualIncome,
            Tracked = balance.ActualTrackedIncome,
            Untracked = balance.ActualIncome - balance.ActualTrackedIncome,
        },
        UnplannedIncome = new IncomeAmountModel
        {
            Total = balance.ActualUnplannedIncome,
            Tracked = balance.ActualUnplannedTrackedIncome,
            Untracked = balance.ActualUnplannedIncome - balance.ActualUnplannedTrackedIncome,
        },
        PlannedGoalContributions = balance.PlannedGoalContributions,
        ExpectedGoalContributions = balance.ExpectedGoalContributions,
        ActualGoalContributions = balance.ActualGoalContributions,
        ActualUnplannedGoalContributions = balance.ActualUnplannedGoalContributions,
    };

    /// <summary>
    /// Converts the provided Accounting Period Balance page to a collection model.
    /// </summary>
    public CollectionModel<AccountingPeriodWithBalanceModel> ToModel(QueryPage<AccountingPeriodBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Accounting Period and its Expected Income Sources to an API model.
    /// </summary>
    public AccountingPeriodWithExpectedIncomeSourcesModel ToModel(AccountingPeriodExpectedIncome expectedIncome) => new()
    {
        Id = expectedIncome.AccountingPeriod.Id.Value,
        Name = expectedIncome.AccountingPeriod.Name,
        Year = expectedIncome.AccountingPeriod.Year,
        Month = expectedIncome.AccountingPeriod.Month,
        IsOpen = expectedIncome.AccountingPeriod.IsOpen,
        ExpectedIncomeSources = expectedIncome.AccountingPeriod.ExpectedIncomeSources
            .Select(source => accountingPeriodConverter.ToModel(source, expectedIncome.ReceivedIncome.Get(source.Id)))
            .ToList(),
    };

    /// <summary>
    /// Converts the provided Expected Income Source detail to an API model.
    /// </summary>
    public ExpectedIncomeSourceDetailModel ToModel(ExpectedIncomeSourceDetail detail) => new()
    {
        AccountingPeriod = ToModel(detail.AccountingPeriod),
        Source = accountingPeriodConverter.ToModel(detail.Source, detail.Received),
        Income = detail.Income.Select(income => new ExpectedIncomeSourceIncomeModel
        {
            TransactionId = income.TransactionId,
            Date = income.Date,
            Description = income.Description,
            Amount = income.Amount,
            ReceivedAmount = new IncomeAmountModel
            {
                Total = income.Received.Total,
                Tracked = income.Received.Tracked,
                Untracked = income.Received.Total - income.Received.Tracked,
            },
        }).ToList(),
    };

    /// <summary>
    /// Converts the provided Accounting Period detail to an API model.
    /// </summary>
    public AccountingPeriodDetailModel ToModel(AccountingPeriodDetail detail) => new()
    {
        AccountingPeriod = ToModel(detail.Balance),
        PreviousPeriod = detail.PreviousPeriod == null ? null : ToModel(detail.PreviousPeriod),
        NextPeriod = detail.NextPeriod == null ? null : ToModel(detail.NextPeriod),
        TotalSpending = detail.TotalSpending,
        AccountOpeningBalance = AccountConverter.ToModel(detail.AccountOpeningBalance),
        AccountClosingBalance = AccountConverter.ToModel(detail.AccountClosingBalance),
        FundOpeningBalance = FundConverter.ToModel(detail.FundOpeningBalance),
        FundClosingBalance = FundConverter.ToModel(detail.FundClosingBalance),
        Accounts = detail.Accounts.Select(ToModel).ToList(),
        Funds = detail.Funds.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts an Account's balances and goal progress in an Accounting Period to an API model.
    /// </summary>
    public AccountingPeriodAccountModel ToModel(AccountingPeriodAccount item) => new()
    {
        Account = accountConverter.ToModel(item.Account),
        OpeningBalance = item.OpeningBalance,
        ClosingBalance = item.ClosingBalance,
        GoalProgress = item.GoalProgress == null ? null : accountGoalConverter.ToModel(item.GoalProgress),
    };

    /// <summary>
    /// Converts a Fund's balances and goal progress in an Accounting Period to an API model.
    /// </summary>
    public AccountingPeriodFundModel ToModel(AccountingPeriodFund item) => new()
    {
        Fund = fundConverter.ToModel(item.Fund),
        OpeningBalance = item.OpeningBalance,
        ClosingBalance = item.ClosingBalance,
        GoalProgress = item.GoalProgress == null ? null : fundGoalConverter.ToModel(item.GoalProgress),
    };

    /// <summary>
    /// Converts the provided Accounting Period range to an API model.
    /// </summary>
    public AccountingPeriodsInRangeModel ToModel(AccountingPeriodRange range) => new()
    {
        AccountingPeriods = range.AccountingPeriods.Select(ToModel).ToList(),
        TotalSpending = range.TotalSpending,
    };

    /// <summary>
    /// Converts the provided Accounting Period Filter model to a Domain filter.
    /// </summary>
    private static AccountingPeriodFilter ToDomain(AccountingPeriodFilterModel? model) => new(
        model?.Years ?? [],
        model?.Months ?? [],
        model?.IsOpen);

    private static IncomeAmountModel ToExpectedIncomeAmountModel(IReadOnlyCollection<ExpectedIncomeSource> sources) => new()
    {
        Total = sources.Sum(source => source.ExpectedAmount),
        Tracked = sources.Sum(source => source.ExpectedTrackedAmount),
        Untracked = sources.Sum(source => source.ExpectedUntrackedAmount),
    };

    private static AccountingPeriodBalanceSort ToDomain(AccountingPeriodWithBalanceSortModel? sort) => sort switch
    {
        AccountingPeriodWithBalanceSortModel.Date => AccountingPeriodBalanceSort.Date,
        AccountingPeriodWithBalanceSortModel.DateDescending => AccountingPeriodBalanceSort.DateDescending,
        AccountingPeriodWithBalanceSortModel.IsOpen => AccountingPeriodBalanceSort.IsOpen,
        AccountingPeriodWithBalanceSortModel.IsOpenDescending => AccountingPeriodBalanceSort.IsOpenDescending,
        AccountingPeriodWithBalanceSortModel.OpeningBalance => AccountingPeriodBalanceSort.OpeningBalance,
        AccountingPeriodWithBalanceSortModel.OpeningBalanceDescending => AccountingPeriodBalanceSort.OpeningBalanceDescending,
        AccountingPeriodWithBalanceSortModel.ClosingBalance => AccountingPeriodBalanceSort.ClosingBalance,
        AccountingPeriodWithBalanceSortModel.ClosingBalanceDescending => AccountingPeriodBalanceSort.ClosingBalanceDescending,
        _ => AccountingPeriodBalanceSort.DateDescending,
    };
}
