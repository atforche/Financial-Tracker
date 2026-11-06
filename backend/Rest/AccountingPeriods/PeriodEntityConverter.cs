using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Rest.AccountGoals;
using Rest.Accounts;
using Rest.FundGoals;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converts one Account or Fund's detail in an Accounting Period to API models.
/// </summary>
public sealed class PeriodEntityConverter(
    AccountingPeriodQueryConverter accountingPeriodQueryConverter,
    AccountGoalConverter accountGoalConverter,
    FundGoalConverter fundGoalConverter,
    AccountBalanceEventConverter accountBalanceEventConverter,
    FundGoalBalanceEventConverter fundGoalBalanceEventConverter)
{
    /// <summary>
    /// Converts the provided Account detail to an API model.
    /// </summary>
    public PeriodAccountDetailModel ToModel(PeriodAccountDetail detail) => new()
    {
        AccountingPeriod = accountingPeriodQueryConverter.ToModel(detail.AccountingPeriod),
        PreviousPeriod = ToOptionalModel(detail.PreviousPeriod),
        NextPeriod = ToOptionalModel(detail.NextPeriod),
        TotalInflow = detail.TotalInflow,
        TotalOutflow = detail.TotalOutflow,
        DailyBalances = ToModel(detail.DailyBalances),
        Account = accountingPeriodQueryConverter.ToModel(detail.Account),
        Goal = detail.Goal == null ? null : accountGoalConverter.ToModel(detail.Goal),
        Events = detail.Events.Select(accountBalanceEventConverter.ToModel).ToList(),
    };

    /// <summary>
    /// Converts the provided Fund detail to an API model.
    /// </summary>
    public PeriodFundDetailModel ToModel(PeriodFundDetail detail) => new()
    {
        AccountingPeriod = accountingPeriodQueryConverter.ToModel(detail.AccountingPeriod),
        PreviousPeriod = ToOptionalModel(detail.PreviousPeriod),
        NextPeriod = ToOptionalModel(detail.NextPeriod),
        TotalInflow = detail.TotalInflow,
        TotalOutflow = detail.TotalOutflow,
        DailyBalances = ToModel(detail.DailyBalances),
        Fund = accountingPeriodQueryConverter.ToModel(detail.Fund),
        Goal = detail.Goal == null ? null : fundGoalConverter.ToModel(detail.Goal),
        Events = detail.Events.Select(fundGoalBalanceEventConverter.ToModel).ToList(),
    };

    /// <summary>
    /// Converts an Accounting Period that may not exist to an API model.
    /// </summary>
    private AccountingPeriodModel? ToOptionalModel(AccountingPeriod? accountingPeriod) =>
        accountingPeriod == null ? null : accountingPeriodQueryConverter.ToModel(accountingPeriod);

    /// <summary>
    /// Converts daily balances to an API model.
    /// </summary>
    private static PeriodBalanceDateRangeModel ToModel(PeriodBalanceDateRange range) => new()
    {
        OpeningBalance = range.OpeningBalance,
        Dates = range.Dates.Select(item => new PeriodBalanceDateModel
        {
            Date = item.Date,
            TotalBalance = item.Balance,
        }).ToList(),
    };
}
