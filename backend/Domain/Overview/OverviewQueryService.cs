using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts.Queries;
using Domain.Funds.Queries;

namespace Domain.Overview;

/// <summary>
/// Service for querying the Account and Fund balance summaries shown on the overview.
/// </summary>
public sealed class OverviewQueryService(
    IOverviewQueryRepository overviewQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves Account and Fund balance summaries over a date range.
    /// </summary>
    public async Task<OverviewDateRange> GetDateRangeAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<OverviewAccountOpeningBalance> accounts =
            await overviewQueryRepository.GetAccountOpeningBalancesAsync(start, cancellationToken);
        IReadOnlyCollection<AccountDateBalanceFact> accountHistory =
            await overviewQueryRepository.GetAccountBalanceHistoryAsync(start, end, cancellationToken);
        IReadOnlyCollection<OverviewFundOpeningBalance> funds =
            await overviewQueryRepository.GetFundOpeningBalancesAsync(start, cancellationToken);
        IReadOnlyCollection<FundDateBalanceFact> fundHistory =
            await overviewQueryRepository.GetFundBalanceHistoryAsync(start, end, cancellationToken);

        BalanceGranularity granularity = BalanceSeries.GetGranularity(start, end);
        List<DateOnly> sampleDates = BalanceSeries.GetSampleDates(start, end, granularity);
        List<(DateOnly Date, AccountBalanceSummary Summary)> accountSummaries = BalanceSeries.Summarize(
            sampleDates,
            accounts.ToDictionary(account => account.AccountId, account => account.Balance),
            accountHistory.Select(item => (item.AccountId, item.Date, item.Sequence, item.PostedBalance)),
            (date, currentBalances) => (date, AccountBalanceSummary.Create(accounts.Select(account => (
                account.Type,
                account.DateOpened is DateOnly opened && date < opened ? 0 : currentBalances[account.AccountId])))));
        List<(DateOnly Date, FundBalanceSummary Summary)> fundSummaries = BalanceSeries.Summarize(
            sampleDates,
            funds.ToDictionary(fund => fund.FundId, fund => fund.Balance),
            fundHistory.Select(item => (item.FundId, item.Date, item.Sequence, item.PostedBalance)),
            (date, currentBalances) => (date, FundBalanceSummary.Create(
                funds.Select(fund => (fund.FundId, currentBalances[fund.FundId])))));
        return new OverviewDateRange(
            accountSummaries[0].Summary,
            accountSummaries[^1].Summary,
            accountSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Summary.TotalBalance)).ToList(),
            fundSummaries[0].Summary,
            fundSummaries[^1].Summary,
            fundSummaries.Skip(1).Select(item => new OverviewDateBalance(item.Date, item.Summary.TotalBalance)).ToList(),
            granularity);
    }

    /// <summary>
    /// Retrieves Account and Fund balance summaries over an Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<OverviewAccountingPeriodRange>> GetAccountingPeriodRangeAsync(
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            startId,
            endId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<OverviewAccountingPeriodRange>(null, resolution.Failure);
        }

        var periods = resolution.AccountingPeriods
            .OrderBy(period => period.Year)
            .ThenBy(period => period.Month)
            .ToList();
        IReadOnlyCollection<AccountingPeriodId> periodIds = periods.Select(period => period.Id).ToList();
        ILookup<AccountingPeriodId, OverviewAccountPeriodBalance> accountBalances =
            (await overviewQueryRepository.GetAccountPeriodBalancesAsync(periodIds, cancellationToken))
            .ToLookup(balance => balance.AccountingPeriodId);
        ILookup<AccountingPeriodId, OverviewFundPeriodBalance> fundBalances =
            (await overviewQueryRepository.GetFundPeriodBalancesAsync(periodIds, cancellationToken))
            .ToLookup(balance => balance.AccountingPeriodId);

        var accountSummaries = periods.Select(period => (
            Period: period,
            Opening: AccountBalanceSummary.Create(accountBalances[period.Id].Select(balance => (balance.Type, balance.OpeningBalance))),
            Closing: AccountBalanceSummary.Create(accountBalances[period.Id].Select(balance => (balance.Type, balance.ClosingBalance)))))
            .ToList();
        var fundSummaries = periods.Select(period => (
            Period: period,
            Opening: FundBalanceSummary.Create(fundBalances[period.Id].Select(balance => (balance.FundId, balance.OpeningBalance))),
            Closing: FundBalanceSummary.Create(fundBalances[period.Id].Select(balance => (balance.FundId, balance.ClosingBalance)))))
            .ToList();
        var range = new OverviewAccountingPeriodRange(
            accountSummaries[0].Opening,
            accountSummaries[^1].Closing,
            accountSummaries.Select(item => new OverviewPeriodBalance(item.Period, item.Opening.TotalBalance, item.Closing.TotalBalance)).ToList(),
            fundSummaries[0].Opening,
            fundSummaries[^1].Closing,
            fundSummaries.Select(item => new OverviewPeriodBalance(item.Period, item.Opening.TotalBalance, item.Closing.TotalBalance)).ToList());
        return new AccountingPeriodRangeQueryResult<OverviewAccountingPeriodRange>(range, AccountingPeriodRangeQueryFailure.None);
    }
}
