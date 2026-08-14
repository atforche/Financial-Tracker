using Domain;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Models;
using Models.AccountingPeriods;
using Rest.Transactions;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converts between Accounting Period API models and Domain query types.
/// </summary>
public sealed class AccountingPeriodQueryConverter(
    TransactionConverter transactionConverter,
    AccountingPeriodConverter accountingPeriodConverter)
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
        model.Sort switch
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
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Accounting Period range query model to a Domain query.
    /// </summary>
    public AccountingPeriodRangeQuery ToDomain(AccountingPeriodsInRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an Accounting Period Transactions query model to a Domain query.
    /// </summary>
    public AccountingPeriodTransactionsQuery ToDomain(
        Guid accountingPeriodId,
        AccountingPeriodWithTransactionsQueryParameterModel model) => new(
            accountingPeriodId,
            transactionConverter.ToDomain(model.Sort),
            model.Offset ?? 0,
            model.Limit);

    /// <summary>
    /// Converts the provided Accounting Period to an API model.
    /// </summary>
    public AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Name = accountingPeriod.Name,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen,
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
        ExpectedIncomeSources = balance.AccountingPeriod.ExpectedIncomeSources.Select(accountingPeriodConverter.ToModel).ToList(),
        ExpectedIncome = ToExpectedIncomeAmountModel(balance.AccountingPeriod.ExpectedIncomeSources),
        ActualIncome = new IncomeAmountModel
        {
            Total = balance.ActualIncome,
            Tracked = balance.ActualTrackedIncome,
            Untracked = balance.ActualIncome - balance.ActualTrackedIncome,
        },
        ExpectedGoalContributions = balance.ExpectedGoalContributions,
        ActualGoalContributions = balance.ActualGoalContributions,
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
    /// Converts the provided Accounting Period range to an API model.
    /// </summary>
    public AccountingPeriodsInRangeModel ToModel(AccountingPeriodRange range) => new()
    {
        AccountingPeriods = ToModel(range.AccountingPeriods),
        TotalIncome = new IncomeAmountModel
        {
            Total = range.TotalIncome,
            Tracked = range.TrackedIncome,
            Untracked = range.UntrackedIncome,
        },
        TotalSpending = range.TotalSpending,
    };

    /// <summary>
    /// Converts interpreted Accounting Period Transactions to an API model.
    /// </summary>
    public AccountingPeriodWithTransactionsModel ToModel(AccountingPeriodTransactions result) => new()
    {
        Id = result.Balance.AccountingPeriod.Id.Value,
        Name = result.Balance.AccountingPeriod.Name,
        Year = result.Balance.AccountingPeriod.Year,
        Month = result.Balance.AccountingPeriod.Month,
        IsOpen = result.Balance.AccountingPeriod.IsOpen,
        OpeningBalance = result.Balance.OpeningBalance,
        ClosingBalance = result.Balance.ClosingBalance,
        ExpectedIncomeSources = result.Balance.AccountingPeriod.ExpectedIncomeSources.Select(accountingPeriodConverter.ToModel).ToList(),
        ExpectedIncome = ToExpectedIncomeAmountModel(result.Balance.AccountingPeriod.ExpectedIncomeSources),
        ActualIncome = new IncomeAmountModel
        {
            Total = result.Balance.ActualIncome,
            Tracked = result.Balance.ActualTrackedIncome,
            Untracked = result.Balance.ActualIncome - result.Balance.ActualTrackedIncome,
        },
        ExpectedGoalContributions = result.Balance.ExpectedGoalContributions,
        ActualGoalContributions = result.Balance.ActualGoalContributions,
        Transactions = transactionConverter.ToModel(result.Transactions),
        TotalIncome = new IncomeAmountModel
        {
            Total = result.TotalIncome,
            Tracked = result.TrackedIncome,
            Untracked = result.UntrackedIncome,
        },
        TotalSpending = result.TotalSpending,
    };

    /// <summary>
    /// Converts the provided Accounting Period Filter model to a Domain filter.
    /// </summary>
    private static AccountingPeriodFilter ToDomain(AccountingPeriodFilterModel? model) => new(
        model?.Years ?? [],
        model?.Months ?? []);

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