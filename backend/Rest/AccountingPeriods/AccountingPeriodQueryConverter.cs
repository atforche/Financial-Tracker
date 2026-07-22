using Domain;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converts between Accounting Period API models and Domain query types.
/// </summary>
public sealed class AccountingPeriodQueryConverter
{
    /// <summary>Converts the provided Accounting Period query model to a Domain query.</summary>
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

    /// <summary>Converts the provided Accounting Period Balance query model to a Domain query.</summary>
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

    /// <summary>Converts the provided Accounting Period to an API model.</summary>
    public AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Name = accountingPeriod.Name,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen,
    };

    /// <summary>Converts the provided Accounting Period page to a collection model.</summary>
    public CollectionModel<AccountingPeriodModel> ToModel(QueryPage<AccountingPeriod> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>Converts the provided Accounting Period Balance to an API model.</summary>
    public AccountingPeriodWithBalanceModel ToModel(AccountingPeriodBalance balance) => new()
    {
        Id = balance.AccountingPeriod.Id.Value,
        Name = balance.AccountingPeriod.Name,
        Year = balance.AccountingPeriod.Year,
        Month = balance.AccountingPeriod.Month,
        IsOpen = balance.AccountingPeriod.IsOpen,
        OpeningBalance = balance.OpeningBalance,
        ClosingBalance = balance.ClosingBalance,
    };

    /// <summary>Converts the provided Accounting Period Balance page to a collection model.</summary>
    public CollectionModel<AccountingPeriodWithBalanceModel> ToModel(QueryPage<AccountingPeriodBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Accounting Period Filter model to a Domain filter.
    /// </summary>
    private static AccountingPeriodFilter ToDomain(AccountingPeriodFilterModel? model) => new(
        model?.Years ?? [],
        model?.Months ?? []);
}