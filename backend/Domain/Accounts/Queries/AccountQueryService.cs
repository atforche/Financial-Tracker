using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying Accounts and their Balances.
/// </summary>
public sealed class AccountQueryService(
    IAccountQueryRepository accountQueryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService,
    PendingAccountBalanceService pendingAccountBalanceService)
{
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
            pendingAccountBalanceService.ApplyPendingEffects(page.Items),
            page.TotalCount);
    }

    /// <summary>
    /// Retrieves Account balances and financial totals over an Accounting Period range.
    /// </summary>
    public async Task<AccountAccountingPeriodRangeQueryResult> GetAccountingPeriodRangeAsync(
        AccountAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountAccountingPeriodRangeQueryResult(null, resolution.Failure);
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
            return new AccountAccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.NotContiguous);
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
        return new AccountAccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

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
        IReadOnlyCollection<AccountDateBalanceFact> history = await accountQueryRepository.GetDateBalanceFactsAsync(
            balances.Select(balance => balance.Account.Id).ToList(),
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
        IReadOnlyCollection<AccountDateBalanceSummary> dates = SummarizeDates(
            query.Start,
            query.End,
            balances,
            history);
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
            dates);
    }

    /// <summary>
    /// Summarizes the provided Account Period Balances into a single Account Balance Summary.
    /// </summary>
    private static AccountBalanceSummary Summarize(IEnumerable<AccountPeriodBalanceFact> balances, bool opening)
    {
        var values = balances.Select(balance => (
            balance.Account.Type,
            Amount: (balance.Account.Type.IsDebt() ? -1 : 1) * (opening ? balance.OpeningBalance : balance.ClosingBalance))).ToList();
        return new AccountBalanceSummary(
            values.Sum(item => item.Amount),
            values.Where(item => item.Type.IsTracked()).Sum(item => item.Amount),
            values.Where(item => !item.Type.IsTracked()).Sum(item => item.Amount),
            values.GroupBy(item => item.Type).OrderBy(group => group.Key)
                .Select(group => new AccountTypeBalance(group.Key, group.Sum(item => item.Amount))).ToList());
    }

    /// <summary>
    /// Summarizes Account balances for each date by advancing through the in-range history once.
    /// </summary>
    private static List<AccountDateBalanceSummary> SummarizeDates(
        DateOnly start,
        DateOnly end,
        IEnumerable<AccountRangeBalance> balances,
        IReadOnlyCollection<AccountDateBalanceFact> history)
    {
        IReadOnlyCollection<AccountRangeBalance> rangeBalances = balances.ToList();
        var currentBalances = rangeBalances.ToDictionary(
            balance => balance.Account.Id,
            balance => balance.StartingBalance);
        var orderedHistory = history
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Sequence)
            .ToList();
        int historyIndex = 0;
        var summaries = new List<AccountDateBalanceSummary>();
        foreach (DateOnly date in GetDates(start, end))
        {
            while (historyIndex < orderedHistory.Count && orderedHistory[historyIndex].Date <= date)
            {
                AccountDateBalanceFact item = orderedHistory[historyIndex++];
                currentBalances[item.AccountId] = item.PostedBalance;
            }
            var values = rangeBalances.Select(balance =>
            {
                decimal amount = balance.Account.DateOpened is DateOnly opened && date < opened
                    ? 0
                    : currentBalances[balance.Account.Id];
                return (balance.Account.Type, Amount: balance.Account.Type.IsDebt() ? -amount : amount);
            }).ToList();
            summaries.Add(new AccountDateBalanceSummary(date, new AccountBalanceSummary(
                values.Sum(item => item.Amount),
                values.Where(item => item.Type.IsTracked()).Sum(item => item.Amount),
                values.Where(item => !item.Type.IsTracked()).Sum(item => item.Amount),
                values.GroupBy(item => item.Type).OrderBy(group => group.Key)
                    .Select(group => new AccountTypeBalance(group.Key, group.Sum(item => item.Amount))).ToList())));
        }
        return summaries;
    }

    /// <summary>
    /// Generates a sequence of dates from the start date to the end date, inclusive.
    /// </summary>
    private static IEnumerable<DateOnly> GetDates(DateOnly start, DateOnly end)
    {
        for (DateOnly date = start; date <= end; date = date.AddDays(1))
        {
            yield return date;
            if (date == end)
            {
                yield break;
            }
        }
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