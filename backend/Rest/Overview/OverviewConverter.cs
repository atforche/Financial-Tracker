using Domain.Overview;
using Models.Overview;
using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Overview;

/// <summary>
/// Converter class that handles converting overview results to API models.
/// </summary>
public sealed class OverviewConverter(AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Converts the provided overview date range to an API model.
    /// </summary>
    public OverviewDateRangeModel ToModel(OverviewDateRange range) => new()
    {
        AccountOpeningBalance = AccountConverter.ToModel(range.AccountOpeningBalance),
        AccountClosingBalance = AccountConverter.ToModel(range.AccountClosingBalance),
        AccountBalances = range.AccountBalances.Select(ToModel).ToList(),
        FundOpeningBalance = FundConverter.ToModel(range.FundOpeningBalance),
        FundClosingBalance = FundConverter.ToModel(range.FundClosingBalance),
        FundBalances = range.FundBalances.Select(ToModel).ToList(),
        Granularity = BalanceGranularityConverter.ToModel(range.Granularity),
    };

    /// <summary>
    /// Converts the provided overview Accounting Period range to an API model.
    /// </summary>
    public OverviewAccountingPeriodRangeModel ToModel(OverviewAccountingPeriodRange range) => new()
    {
        AccountOpeningBalance = AccountConverter.ToModel(range.AccountOpeningBalance),
        AccountClosingBalance = AccountConverter.ToModel(range.AccountClosingBalance),
        AccountBalances = range.AccountBalances.Select(ToModel).ToList(),
        FundOpeningBalance = FundConverter.ToModel(range.FundOpeningBalance),
        FundClosingBalance = FundConverter.ToModel(range.FundClosingBalance),
        FundBalances = range.FundBalances.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts the provided date balance to an API model.
    /// </summary>
    private static OverviewDateBalanceModel ToModel(OverviewDateBalance balance) => new()
    {
        Date = balance.Date,
        TotalBalance = balance.TotalBalance,
    };

    /// <summary>
    /// Converts the provided Accounting Period balance to an API model.
    /// </summary>
    private OverviewPeriodBalanceModel ToModel(OverviewPeriodBalance balance) => new()
    {
        AccountingPeriod = accountingPeriodConverter.ToModel(balance.AccountingPeriod),
        OpeningBalance = balance.OpeningBalance,
        ClosingBalance = balance.ClosingBalance,
    };
}
