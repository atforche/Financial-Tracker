using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;

namespace Domain.Accounts.Queries;

/// <summary>
/// Service for querying Accounts and their Balances.
/// </summary>
public sealed class AccountQueryService(
    IAccountRepository accountRepository,
    IAccountQueryRepository accountQueryRepository,
    IAccountingPeriodQueryRepository accountingPeriodQueryRepository)
{
    /// <summary>
    /// Retrieves the Account with the specified ID, or null when it does not exist.
    /// </summary>
    public Account? GetById(Guid accountId)
    {
        if (accountRepository.TryGetById(accountId, out Account? account))
        {
            return account;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the Accounts matching the provided query.
    /// </summary>
    public Task<QueryPage<Account>> GetAsync(AccountQuery query, CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Accounts and their interpreted current balances.
    /// </summary>
    public Task<QueryPage<AccountBalance>> GetWithBalancesAsync(
        AccountBalanceQuery query,
        CancellationToken cancellationToken = default) =>
        accountQueryRepository.GetBalancesAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves Account balances and financial totals over an Accounting Period range.
    /// </summary>
    public async Task<AccountAccountingPeriodRangeQueryResult> GetAccountingPeriodRangeAsync(
        AccountAccountingPeriodRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<AccountingPeriod> endpoints = await accountingPeriodQueryRepository.GetEndpointsAsync(
            query.StartId,
            query.EndId,
            cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == query.StartId);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == query.EndId);
        AccountingPeriodRangeQueryFailure failure = AccountingPeriodRangeQueryFailure.None;
        if (start == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.StartNotFound;
        }
        if (end == null)
        {
            failure |= AccountingPeriodRangeQueryFailure.EndNotFound;
        }
        if (failure != AccountingPeriodRangeQueryFailure.None)
        {
            return new AccountAccountingPeriodRangeQueryResult(null, failure);
        }

        int startIndex = GetChronologicalIndex(start!);
        int endIndex = GetChronologicalIndex(end!);
        if (startIndex > endIndex)
        {
            return new AccountAccountingPeriodRangeQueryResult(null, AccountingPeriodRangeQueryFailure.Reversed);
        }

        IReadOnlyCollection<AccountPeriodBalanceFacts> histories = await accountQueryRepository.GetPeriodBalanceFactsAsync(
            startIndex,
            endIndex,
            cancellationToken);
        IReadOnlyCollection<AccountPeriodBalanceFacts> orderedHistories = histories
            .OrderBy(history => history.AccountingPeriod.Year)
            .ThenBy(history => history.AccountingPeriod.Month)
            .ToList();
        IReadOnlyCollection<int> indexes = orderedHistories.Select(history => GetChronologicalIndex(history.AccountingPeriod)).ToList();
        if (!indexes.SequenceEqual(Enumerable.Range(startIndex, endIndex - startIndex + 1)))
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
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> incomeFacts = await accountingPeriodQueryRepository.GetRangeIncomeFactsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeSpendingFact> spendingFacts = await accountingPeriodQueryRepository.GetRangeSpendingFactsAsync(periodIds, cancellationToken);
        IReadOnlyCollection<AccountingPeriodRangeIncomeFact> recognizedIncome = incomeFacts
            .Where(fact => !fact.HasInternalSource || fact.PostedDate != null)
            .ToList();
        decimal totalIncome = recognizedIncome.Sum(fact => fact.Amount);
        decimal trackedIncome = recognizedIncome.Where(fact => fact.AccountType.IsTracked()).Sum(fact => fact.Amount);
        decimal totalSpending = spendingFacts.Where(fact => fact.PostedDate != null).Sum(fact => fact.Amount);
        IReadOnlyCollection<AccountPeriodBalanceSummary> summaries = orderedHistories.Select(history => new AccountPeriodBalanceSummary(
            history.AccountingPeriod,
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Account.Id)), true),
            Summarize(history.Balances.Where(balance => matchingIds.Contains(balance.Account.Id)), false))).ToList();
        var range = new AccountAccountingPeriodRange(
            new QueryPage<AccountRangeBalance>(items, balances.Count),
            await accountQueryRepository.GetAllNamesAsync(cancellationToken),
            totalIncome,
            trackedIncome,
            totalSpending,
            summaries);
        return new AccountAccountingPeriodRangeQueryResult(range, AccountingPeriodRangeQueryFailure.None);
    }

    /// <summary>
    /// Retrieves the chronological index for the provided Accounting Period.
    /// </summary>
    private static int GetChronologicalIndex(AccountingPeriod period) => (period.Year * 12) + period.Month;

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