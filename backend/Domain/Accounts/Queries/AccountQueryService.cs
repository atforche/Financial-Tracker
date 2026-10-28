using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying Accounts and their Balances.
/// </summary>
public sealed class AccountQueryService(
    IAccountQueryRepository accountQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    PendingAccountBalanceService pendingAccountBalanceService,
    IAccountBalanceEventQueryRepository balanceEventQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository)
{
    /// <summary>
    /// Returns daily posted Account balances using only movements assigned to the selected Accounting Period.
    /// </summary>
    public async Task<PeriodBalanceDateRange?> GetPeriodBalanceDatesAsync(
        Guid accountId, Guid accountingPeriodId, CancellationToken cancellationToken = default)
    {
        AccountId id = new(accountId);
        AccountingPeriodId periodId = new(accountingPeriodId);
        Account? account = await accountQueryRepository.GetByIdAsync(id, cancellationToken);
        AccountingPeriod? period = await accountingPeriodQueryRepository.GetByIdAsync(periodId, cancellationToken);
        if (account == null || period == null)
        {
            return null;
        }
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(periodId);
        AccountingPeriodAccountBalanceHistory? accountBalance = balanceHistory.AccountBalances.SingleOrDefault(item => item.Account.Id == id);
        if (accountBalance == null)
        {
            return null;
        }
        IReadOnlyCollection<AccountBalanceHistory> histories = await balanceEventQueryRepository.GetPeriodHistoriesAsync(id, periodId, cancellationToken);
        return PeriodBalanceDateSeries.Build(period, accountBalance.OpeningBalance,
            histories.Select(item => (item.Date, item.Sequence, item.AccountingPeriodBalanceChange)));
    }

    /// <summary>
    /// Retrieves the Account with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<Account?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetByIdAsync(new AccountId(accountId), cancellationToken);

    /// <summary>
    /// Retrieves the Accounts matching the provided query.
    /// </summary>
    public Task<QueryPage<Account>> GetAsync(AccountQuery query, CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves all available Account financial institutions.
    /// </summary>
    public Task<IReadOnlyCollection<string>> GetAllFinancialInstitutionsAsync(CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetAllFinancialInstitutionsAsync(cancellationToken);

    /// <summary>
    /// Retrieves Accounts and their interpreted current balances.
    /// </summary>
    public async Task<QueryPage<AccountBalance>> GetWithBalancesAsync(
        AccountBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        QueryPage<AccountBalance> page = await accountQueryRepository.GetBalancesAsync(query, cancellationToken);
        return new QueryPage<AccountBalance>(
            pendingAccountBalanceService.IncludePendingAmounts(page.Items),
            page.TotalCount);
    }

    /// <summary>
    /// Retrieves Account balances and financial totals over an Accounting Period range.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<AccountAccountingPeriodRange>> GetAccountingPeriodRangeAsync(
        AccountAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<AccountAccountingPeriodRange>(null, resolution.Failure);
        }

        AccountingPeriod start = resolution.AccountingPeriods.First();
        AccountingPeriod end = resolution.AccountingPeriods.Last();
        IReadOnlyCollection<AccountPeriodBalanceFacts> histories = await accountQueryRepository.GetPeriodBalanceFactsAsync(
            AccountingPeriodRangeResolver.GetChronologicalIndex(start),
            AccountingPeriodRangeResolver.GetChronologicalIndex(end),
            cancellationToken);
        IReadOnlyCollection<AccountPeriodBalanceFacts> orderedHistories = histories
            .OrderBy(history => history.AccountingPeriod.Year)
            .ThenBy(history => history.AccountingPeriod.Month)
            .ToList();
        if (!AccountingPeriodRangeResolver.IsContiguous(orderedHistories.Select(history => history.AccountingPeriod), start, end))
        {
            return new AccountingPeriodRangeQueryResult<AccountAccountingPeriodRange>(null, AccountingPeriodRangeQueryFailure.NotContiguous);
        }

        IReadOnlyCollection<Account> accounts = await accountQueryRepository.GetRangeAccountsAsync(query.Filter, cancellationToken);
        var matchingIds = accounts.Select(account => account.Id).ToHashSet();
        AccountPeriodBalanceFacts first = orderedHistories.First();
        AccountPeriodBalanceFacts last = orderedHistories.Last();
        var balances = accounts.Select(account => new AccountRangeBalance(
            account,
            first.Balances.SingleOrDefault(balance => balance.Account.Id == account.Id)?.OpeningBalance ?? account.OnboardedBalance ?? 0,
            last.Balances.SingleOrDefault(balance => balance.Account.Id == account.Id)?.ClosingBalance ?? account.OnboardedBalance ?? 0)).ToList();
        IReadOnlyCollection<AccountRangeBalance> items = Sort(balances, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        IReadOnlyCollection<Guid> periodIds = orderedHistories.Select(history => history.AccountingPeriod.Id.Value).ToList();
        IReadOnlyCollection<AccountId> accountIds = accounts.Select(account => account.Id).ToList();
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await accountQueryRepository.GetAccountingPeriodRangeIncomeFactsAsync(accountIds, periodIds, cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountQueryRepository.GetAccountingPeriodRangeSpendingFactsAsync(accountIds, periodIds, cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        IReadOnlyCollection<AccountPeriodBalanceSummary> summaries = orderedHistories.Select(history => new AccountPeriodBalanceSummary(
            history.AccountingPeriod,
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Account.Id)), true),
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Account.Id)), false))).ToList();
        var range = new AccountAccountingPeriodRange(
            new QueryPage<AccountRangeBalance>(items, balances.Count),
            await accountQueryRepository.GetAllNamesAsync(cancellationToken),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending,
            summaries);
        return new AccountingPeriodRangeQueryResult<AccountAccountingPeriodRange>(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Retrieves Account balance summaries for each date sampled between the provided dates, and the spacing between them.
    /// </summary>
    public async Task<(List<AccountDateBalanceSummary> Summaries, BalanceGranularity Granularity)> GetDateSummariesAsync(
        AccountFilter filter,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default) =>
        await SummarizeDatesAsync(
            await accountQueryRepository.GetDateRangeBalancesAsync(filter, start, end, cancellationToken),
            start,
            end,
            cancellationToken);

    /// <summary>
    /// Retrieves Account balances and financial totals over a date range.
    /// </summary>
    public async Task<AccountDateRange> GetDateRangeAsync(
        AccountDateRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountRangeBalance> balances = await accountQueryRepository.GetDateRangeBalancesAsync(
            query.Filter,
            query.Start,
            query.End,
            cancellationToken);
        IReadOnlyCollection<AccountId> accountIds = balances.Select(balance => balance.Account.Id).ToList();
        IReadOnlyCollection<FinancialRangeIncomeFact> incomeFacts = await accountQueryRepository.GetDateRangeIncomeFactsAsync(
            accountIds,
            query.Start,
            query.End,
            cancellationToken);
        IReadOnlyCollection<FinancialRangeSpendingFact> spendingFacts = await accountQueryRepository.GetDateRangeSpendingFactsAsync(
            accountIds,
            query.Start,
            query.End,
            cancellationToken);
        var totals = FinancialRangeTotals.Calculate(incomeFacts, spendingFacts);
        BalanceGranularity granularity = BalanceGranularity.Day;
        List<AccountDateBalanceSummary> summaries = [];
        if (query.Start is DateOnly start && query.End is DateOnly end)
        {
            (summaries, granularity) = await SummarizeDatesAsync(balances, start, end, cancellationToken);
        }
        IReadOnlyCollection<AccountRangeBalance> items = Sort(balances, query.Sort)
            .Skip(query.Offset)
            .Take(query.Limit ?? int.MaxValue)
            .ToList();
        return new AccountDateRange(
            new QueryPage<AccountRangeBalance>(items, balances.Count),
            await accountQueryRepository.GetAllNamesAsync(cancellationToken),
            totals.TotalIncome,
            totals.TrackedIncome,
            totals.TotalSpending,
            summaries.FirstOrDefault()?.Balance,
            granularity,
            summaries.Skip(1).ToList());
    }

    /// <summary>
    /// Summarizes the provided Account Period Balances into a single Account Balance Summary.
    /// </summary>
    private static AccountBalanceSummary Summarize(IEnumerable<AccountPeriodBalanceFact> balances, bool opening) =>
        AccountBalanceSummary.Create(balances.Select(balance => (
            balance.Account.Type,
            opening ? balance.OpeningBalance : balance.ClosingBalance)));

    /// <summary>
    /// Summarizes Account balances for each date, treating Accounts as empty before they were opened.
    /// </summary>
    private static List<AccountDateBalanceSummary> SummarizeDates(
        IReadOnlyList<DateOnly> sampleDates,
        IReadOnlyCollection<AccountRangeBalance> balances,
        IReadOnlyCollection<AccountDateBalanceFact> history) =>
        BalanceSeries.Summarize(
            sampleDates,
            balances.ToDictionary(balance => balance.Account.Id, balance => balance.StartingBalance),
            history.Select(item => (item.AccountId, item.Date, item.Sequence, item.PostedBalance)),
            (date, currentBalances) => new AccountDateBalanceSummary(date, AccountBalanceSummary.Create(
                balances.Select(balance => (
                    balance.Account.Type,
                    balance.Account.DateOpened is DateOnly opened && date < opened ? 0 : currentBalances[balance.Account.Id])))));

    /// <summary>
    /// Summarizes the provided Account balances for each date sampled between the provided dates, using the balance
    /// history within those dates, and gets the spacing between the sampled dates.
    /// </summary>
    private async Task<(List<AccountDateBalanceSummary> Summaries, BalanceGranularity Granularity)> SummarizeDatesAsync(
        IReadOnlyCollection<AccountRangeBalance> balances,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<AccountDateBalanceFact> history = await accountQueryRepository.GetDateBalanceFactsAsync(
            balances.Select(balance => balance.Account.Id).ToList(),
            start,
            end,
            cancellationToken);
        BalanceGranularity granularity = BalanceSeries.GetGranularity(start, end);
        return (SummarizeDates(BalanceSeries.GetSampleDates(start, end, granularity), balances, history), granularity);
    }

    /// <summary>
    /// Sorts the provided Account Range Balances according to the provided sort order.
    /// </summary>
    private static IOrderedEnumerable<AccountRangeBalance> Sort(
        IEnumerable<AccountRangeBalance> balances,
        AccountRangeSort sort) => sort switch
        {
            AccountRangeSort.Name => balances.OrderBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.NameDescending => balances.OrderByDescending(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.Type => balances.OrderBy(balance => balance.Account.Type).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.TypeDescending => balances.OrderByDescending(balance => balance.Account.Type).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.StartingBalance => balances.OrderBy(balance => balance.StartingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.StartingBalanceDescending => balances.OrderByDescending(balance => balance.StartingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.EndingBalance => balances.OrderBy(balance => balance.EndingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.EndingBalanceDescending => balances.OrderByDescending(balance => balance.EndingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.NetChange => balances.OrderBy(balance => balance.EndingBalance - balance.StartingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            AccountRangeSort.NetChangeDescending => balances.OrderByDescending(balance => balance.EndingBalance - balance.StartingBalance).ThenBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
            _ => balances.OrderBy(balance => balance.Account.Name).ThenBy(balance => balance.Account.Id),
        };
}
