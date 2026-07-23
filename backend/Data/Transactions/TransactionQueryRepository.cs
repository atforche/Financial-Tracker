using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Transactions;

/// <summary>
/// Entity Framework implementation of Transaction detail fact retrieval.
/// </summary>
public sealed class TransactionQueryRepository(DatabaseContext databaseContext) : ITransactionQueryRepository
{
    /// <inheritdoc/>
    public Task<Transaction?> GetByIdAsync(TransactionId transactionId, CancellationToken cancellationToken = default) =>
        databaseContext.Transactions.AsNoTracking().SingleOrDefaultAsync(transaction => transaction.Id == transactionId, cancellationToken);

    /// <inheritdoc/>
    public async Task<TransactionQueryFacts> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default) =>
        await GetAsync(
            ApplyFilter(databaseContext.Transactions.AsNoTracking(), query.Filter),
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);

    /// <inheritdoc/>
    public async Task<TransactionDateRangeFacts> GetDateRangeAsync(
        TransactionDateRangeQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> filtered = ApplyFilter(databaseContext.Transactions.AsNoTracking(), query.Filter)
            .Where(transaction => transaction.Date >= query.Start && transaction.Date <= query.End);
        return await GetRangeFactsAsync(filtered, query.Sort, query.Offset, query.Limit, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TransactionAccountingPeriodRangeFacts> GetAccountingPeriodRangeAsync(
        TransactionAccountingPeriodRangeQuery query,
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> filtered = ApplyFilter(databaseContext.Transactions.AsNoTracking(), query.Filter)
            .Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId));
        TransactionDateRangeFacts facts = await GetRangeFactsAsync(
            filtered,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new TransactionAccountingPeriodRangeFacts(
            facts.QueryFacts,
            facts.AvailableAccountNames,
            facts.AvailableFundNames,
            facts.TransactionTypes);
    }

    /// <summary>
    /// Retrieves Transaction range metadata and an interpreted page's persisted context.
    /// </summary>
    private async Task<TransactionDateRangeFacts> GetRangeFactsAsync(
        IQueryable<Transaction> filtered,
        TransactionSort sort,
        int offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        List<TransactionTypeSummary> summaries = await filtered.GroupBy(transaction => transaction.Type)
            .Select(group => new TransactionTypeSummary(
                group.Key,
                group.Count(),
                group.Sum(transaction => transaction.Amount)))
            .ToListAsync(cancellationToken);
        List<string> accountNames = await databaseContext.Accounts.AsNoTracking()
            .OrderBy(account => account.Name)
            .Select(account => account.Name)
            .ToListAsync(cancellationToken);
        List<string> fundNames = await databaseContext.Funds.AsNoTracking()
            .OrderBy(fund => fund.Name)
            .Select(fund => fund.Name)
            .ToListAsync(cancellationToken);
        TransactionQueryFacts queryFacts = await GetAsync(
            filtered,
            sort,
            offset,
            limit,
            cancellationToken);
        return new TransactionDateRangeFacts(queryFacts, accountNames, fundNames, summaries);
    }

    /// <summary>
    /// Retrieves and enriches a page from an already-filtered Transaction query.
    /// </summary>
    private async Task<TransactionQueryFacts> GetAsync(
        IQueryable<Transaction> filtered,
        TransactionSort sort,
        int offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        int totalCount = await filtered.CountAsync(cancellationToken);
        IReadOnlyCollection<Transaction> transactions;
        if (sort is TransactionSort.AccountingPeriod or TransactionSort.AccountingPeriodDescending
            or TransactionSort.Source or TransactionSort.SourceDescending
            or TransactionSort.Destination or TransactionSort.DestinationDescending)
        {
            List<Transaction> allTransactions = await filtered.ToListAsync(cancellationToken);
            Dictionary<AccountingPeriodId, int> periodOrder = sort is TransactionSort.AccountingPeriod or TransactionSort.AccountingPeriodDescending
                ? await databaseContext.AccountingPeriods.AsNoTracking()
                    .ToDictionaryAsync(period => period.Id, period => (period.Year * 12) + period.Month, cancellationToken)
                : [];
            transactions = Sort(allTransactions, sort, periodOrder)
                .Skip(offset)
                .Take(limit ?? int.MaxValue)
                .ToList();
        }
        else
        {
            transactions = await ApplySort(filtered, sort)
                .Skip(offset)
                .Take(limit ?? int.MaxValue)
                .ToListAsync(cancellationToken);
        }

        return await GetQueryFactsAsync(transactions, totalCount, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TransactionDetailsFacts?> GetDetailsByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await databaseContext.Transactions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == transactionId, cancellationToken);
        if (transaction == null)
        {
            return null;
        }

        AccountingPeriod period = await databaseContext.AccountingPeriods.AsNoTracking()
            .SingleAsync(item => item.Id == transaction.AccountingPeriodId, cancellationToken);
        IReadOnlyCollection<FundId> fundIds = transaction.GetAllAffectedFundIds(null).Distinct().ToList();
        IReadOnlyCollection<Fund> funds = await databaseContext.Funds.AsNoTracking()
            .Where(fund => fundIds.Contains(fund.Id))
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountId> accountIds = transaction.GetAllAffectedAccountIds().Distinct().ToList();
        IReadOnlyCollection<AccountBalanceHistory> accountHistories = await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => accountIds.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundBalanceHistory> fundHistories = await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.Fund.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundPlanTotalsHistory> fundPlanHistories = await databaseContext.FundPlanTotalsHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        return new TransactionDetailsFacts(
            transaction,
            period,
            funds,
            accountHistories,
            fundHistories,
            fundPlanHistories);
    }

    /// <summary>
    /// Loads batched interpretation context for a Transaction page.
    /// </summary>
    private async Task<TransactionQueryFacts> GetQueryFactsAsync(
        IReadOnlyCollection<Transaction> transactions,
        int totalCount,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<AccountingPeriodId> periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        IReadOnlyCollection<AccountingPeriod> periods = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => periodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundId> fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        IReadOnlyCollection<Fund> funds = await databaseContext.Funds.AsNoTracking()
            .Where(fund => fundIds.Contains(fund.Id))
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountId> accountIds = transactions.SelectMany(transaction => transaction.GetAllAffectedAccountIds()).Distinct().ToList();
        IReadOnlyCollection<AccountBalanceHistory> accountHistories = await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => accountIds.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundBalanceHistory> fundHistories = await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.Fund.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundPlanTotalsHistory> fundPlanHistories = await databaseContext.FundPlanTotalsHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        return new TransactionQueryFacts(
            new QueryPage<Transaction>(transactions, totalCount),
            periods,
            funds,
            accountHistories,
            fundHistories,
            fundPlanHistories);
    }

    /// <summary>
    /// Applies Transaction filters in the database.
    /// </summary>
    private static IQueryable<Transaction> ApplyFilter(IQueryable<Transaction> transactions, TransactionFilter filter)
    {
        if (filter.AccountingPeriodIds.Count > 0)
        {
            var accountingPeriodIds = filter.AccountingPeriodIds.Select(id => new AccountingPeriodId(id)).ToList();
            transactions = transactions.Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId));
        }
        if (filter.AccountIds.Count > 0)
        {
            var accountIds = filter.AccountIds.Select(id => new AccountId(id)).ToList();
            transactions = transactions.Where(transaction =>
                (transaction is SpendingTransaction
                    && (accountIds.Contains(((SpendingTransaction)transaction).Source.Account.Id)
                        || ((SpendingTransaction)transaction).Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id))))
                || (transaction is IncomeTransaction
                    && ((((IncomeTransaction)transaction).Source.Account != null && accountIds.Contains(((IncomeTransaction)transaction).Source.Account!.Id))
                        || ((IncomeTransaction)transaction).Destinations.Any(destination => accountIds.Contains(destination.Account.Id))))
                || (transaction is AccountTransaction
                    && ((((AccountTransaction)transaction).Source.Account != null && accountIds.Contains(((AccountTransaction)transaction).Source.Account!.Id))
                        || ((AccountTransaction)transaction).Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id)))));
        }
        if (filter.FundIds.Count > 0)
        {
            var fundIds = filter.FundIds.Select(id => new FundId(id)).ToList();
            transactions = transactions.Where(transaction =>
                (transaction is SpendingTransaction && ((SpendingTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
                || (transaction is IncomeTransaction && ((IncomeTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
                || (transaction is FundTransaction
                    && (fundIds.Contains(((FundTransaction)transaction).Source.Fund.Id)
                        || ((FundTransaction)transaction).Destinations.Any(destination => fundIds.Contains(destination.Fund.Id)))));
        }
        return transactions;
    }

    /// <summary>
    /// Applies database-supported Transaction sorting.
    /// </summary>
    private static IQueryable<Transaction> ApplySort(IQueryable<Transaction> transactions, TransactionSort sort) => sort switch
    {
        TransactionSort.Date => transactions.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.DateDescending => transactions.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.Description => transactions.OrderBy(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.DescriptionDescending => transactions.OrderByDescending(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.Amount => transactions.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.AmountDescending => transactions.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSort.AccountingPeriod or TransactionSort.AccountingPeriodDescending
            or TransactionSort.Source or TransactionSort.SourceDescending
            or TransactionSort.Destination or TransactionSort.DestinationDescending =>
            transactions.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
    };

    /// <summary>
    /// Applies Transaction sorting that requires materialized related values.
    /// </summary>
    private static IOrderedEnumerable<Transaction> Sort(
        IEnumerable<Transaction> transactions,
        TransactionSort sort,
        Dictionary<AccountingPeriodId, int> periodOrder) => sort switch
        {
            TransactionSort.AccountingPeriod => transactions.OrderBy(transaction => periodOrder[transaction.AccountingPeriodId]).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.AccountingPeriodDescending => transactions.OrderByDescending(transaction => periodOrder[transaction.AccountingPeriodId]).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.Source => transactions.OrderBy(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.SourceDescending => transactions.OrderByDescending(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.Destination => transactions.OrderBy(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.DestinationDescending => transactions.OrderByDescending(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.Date or TransactionSort.DateDescending
                or TransactionSort.Description or TransactionSort.DescriptionDescending
                or TransactionSort.Amount or TransactionSort.AmountDescending =>
                transactions.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
        };

    /// <summary>
    /// Gets the persisted source display value for a Transaction.
    /// </summary>
    private static string? GetSource(Transaction transaction) => transaction switch
    {
        SpendingTransaction spending => spending.Source.Account.Name,
        IncomeTransaction income => income.Source.Account?.Name ?? income.Source.Location,
        AccountTransaction account => account.Source.Account?.Name ?? account.Source.Location,
        FundTransaction fund => fund.Source.Fund.Name,
        _ => null,
    };

    /// <summary>
    /// Gets the persisted destination display value for a Transaction.
    /// </summary>
    private static string GetDestination(Transaction transaction) => transaction switch
    {
        SpendingTransaction spending => string.Join(", ", spending.Destinations.Select(destination => destination.Account?.Name ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        IncomeTransaction income => string.Join(", ", income.Destinations.Select(destination => destination.Account.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransaction account => string.Join(", ", account.Destinations.Select(destination => destination.Account?.Name ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        FundTransaction fund => string.Join(", ", fund.Destinations.Select(destination => destination.Fund.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        _ => string.Empty,
    };
}