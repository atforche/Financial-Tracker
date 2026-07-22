using Domain;
using Domain.Funds;
using Domain.Funds.Queries;
using Models;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Funds to Fund Models
/// </summary>
public sealed class FundConverter
{
    /// <summary>
    /// Converts the provided Fund query model to a Domain query.
    /// </summary>
    public FundQuery ToDomain(FundQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            FundSortModel.Name => FundSort.Name,
            FundSortModel.NameDescending => FundSort.NameDescending,
            FundSortModel.Description => FundSort.Description,
            FundSortModel.DescriptionDescending => FundSort.DescriptionDescending,
            _ => FundSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund Balance query model to a Domain query.
    /// </summary>
    public FundBalanceQuery ToDomain(FundWithBalanceQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            FundWithBalanceSortModel.Name => FundBalanceSort.Name,
            FundWithBalanceSortModel.NameDescending => FundBalanceSort.NameDescending,
            FundWithBalanceSortModel.Description => FundBalanceSort.Description,
            FundWithBalanceSortModel.DescriptionDescending => FundBalanceSort.DescriptionDescending,
            FundWithBalanceSortModel.PostedBalance => FundBalanceSort.PostedBalance,
            FundWithBalanceSortModel.PostedBalanceDescending => FundBalanceSort.PostedBalanceDescending,
            _ => FundBalanceSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund Accounting Period range query to a Domain query.
    /// </summary>
    public FundAccountingPeriodRangeQuery ToDomain(FundsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        model.Sort switch
        {
            FundWithBalanceRangeSortModel.Name => FundRangeSort.Name,
            FundWithBalanceRangeSortModel.NameDescending => FundRangeSort.NameDescending,
            FundWithBalanceRangeSortModel.StartingBalance => FundRangeSort.StartingBalance,
            FundWithBalanceRangeSortModel.StartingBalanceDescending => FundRangeSort.StartingBalanceDescending,
            FundWithBalanceRangeSortModel.EndingBalance => FundRangeSort.EndingBalance,
            FundWithBalanceRangeSortModel.EndingBalanceDescending => FundRangeSort.EndingBalanceDescending,
            FundWithBalanceRangeSortModel.NetChange => FundRangeSort.NetChange,
            FundWithBalanceRangeSortModel.NetChangeDescending => FundRangeSort.NetChangeDescending,
            _ => FundRangeSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description,
    };

    /// <summary>
    /// Converts the provided Fund page to a Fund collection model.
    /// </summary>
    public CollectionModel<FundModel> ToModel(QueryPage<Fund> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts a Fund and its balance to a Fund with Balance model.
    /// </summary>
    public FundWithBalanceModel ToModel(FundBalance balance) => new()
    {
        Id = balance.Fund.Id.Value,
        Name = balance.Fund.Name,
        Description = balance.Fund.Description,
        CurrentBalance = new FundBalanceModel
        {
            PostedBalance = balance.PostedBalance,
            PendingDebitAmount = balance.PendingDebitAmount,
            PendingCreditAmount = balance.PendingCreditAmount,
        },
    };

    /// <summary>
    /// Converts the provided Fund Balance page to a collection model.
    /// </summary>
    public CollectionModel<FundWithBalanceModel> ToModel(QueryPage<FundBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Fund Accounting Period range to an API model.
    /// </summary>
    public FundsInAccountingPeriodRangeModel ToModel(FundAccountingPeriodRange range) => new()
    {
        Funds = new CollectionModel<FundWithBalanceRangeModel>
        {
            Items = range.Funds.Items.Select(balance => new FundWithBalanceRangeModel
            {
                Id = balance.Fund.Id.Value,
                Name = balance.Fund.Name,
                Description = balance.Fund.Description,
                StartingBalance = balance.StartingBalance,
                EndingBalance = balance.EndingBalance,
            }).ToList(),
            TotalCount = range.Funds.TotalCount,
        },
        AvailableFundNames = range.AvailableFundNames,
        TotalIncome = new IncomeAmountModel
        {
            Total = range.TotalIncome,
            Tracked = range.TrackedIncome,
            Untracked = range.UntrackedIncome,
        },
        TotalSpending = range.TotalSpending,
        AccountingPeriods = range.AccountingPeriods.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts the provided Fund Period Balance Summary to a model.
    /// </summary>
    private static FundBalanceSummaryByPeriodModel ToModel(FundPeriodBalanceSummary summary) => new()
    {
        AccountingPeriod = new Models.AccountingPeriods.AccountingPeriodModel
        {
            Id = summary.AccountingPeriod.Id.Value,
            Name = summary.AccountingPeriod.Name,
            Year = summary.AccountingPeriod.Year,
            Month = summary.AccountingPeriod.Month,
            IsOpen = summary.AccountingPeriod.IsOpen,
        },
        OpeningBalance = ToModel(summary.OpeningBalance),
        ClosingBalance = ToModel(summary.ClosingBalance),
    };

    /// <summary>
    /// Converts the provided Fund Balance Summary to a model.
    /// </summary>
    private static FundBalanceSummaryModel ToModel(FundBalanceSummary summary) => new()
    {
        TotalBalance = summary.TotalBalance,
        TotalAssignedBalance = summary.TotalAssignedBalance,
        TotalUnassignedBalance = summary.TotalUnassignedBalance,
    };

    /// <summary>
    /// Converts the provided Fund Filter model to a Domain filter.
    /// </summary>
    private static FundFilter ToDomain(FundFilterModel? model) => new(
        model?.NameSearch,
        model?.Names ?? []);
}