using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Refunds;
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
        databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .SingleOrDefaultAsync(transaction => transaction.Id == transactionId, cancellationToken);

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
        return await GetRangeFactsAsync(filtered, query.Filter.LocationIds, query.Sort, query.Offset, query.Limit, cancellationToken);
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
            query.Filter.LocationIds,
            query.Sort,
            query.Offset,
            query.Limit,
            cancellationToken);
        return new TransactionAccountingPeriodRangeFacts(
            facts.QueryFacts,
            facts.AvailableAccountNames,
            facts.AvailableFundNames,
            facts.TransactionTypes,
            facts.LocationCashFlow);
    }

    /// <summary>
    /// Retrieves Transaction range metadata and an interpreted page's persisted context.
    /// </summary>
    private async Task<TransactionDateRangeFacts> GetRangeFactsAsync(
        IQueryable<Transaction> filtered,
        IReadOnlyCollection<Guid> locationIds,
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
        LocationCashFlow locationCashFlow = await GetLocationCashFlowAsync(filtered, locationIds, cancellationToken);
        return new TransactionDateRangeFacts(queryFacts, accountNames, fundNames, summaries, locationCashFlow);
    }

    /// <summary>
    /// Calculates directional flow for the selected Location endpoints across the full range.
    /// </summary>
    private static async Task<LocationCashFlow> GetLocationCashFlowAsync(
        IQueryable<Transaction> filtered,
        IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
        {
            return new LocationCashFlow(0, 0);
        }
        var selectedIds = locationIds.ToHashSet();
        IReadOnlyCollection<Transaction> transactions = await filtered.AsSplitQuery().ToListAsync(cancellationToken);
        decimal incoming = 0;
        decimal outgoing = 0;
        foreach (Transaction transaction in transactions)
        {
            switch (transaction)
            {
                case IncomeTransaction income when income.Source.Location != null
                    && selectedIds.Contains(income.Source.Location.Id.Value):
                    incoming += income.Amount;
                    break;
                case SpendingTransaction spending:
                    decimal spendingAmount = spending.Destinations
                        .Where(destination => destination.Location != null
                            && selectedIds.Contains(destination.Location.Id.Value))
                        .Sum(destination => destination.Amount);
                    outgoing += spendingAmount;
                    break;
                case RefundTransaction refund:
                    incoming += refund.Sources.Where(source => source.Location != null && selectedIds.Contains(source.Location.Id.Value)).Sum(source => source.Amount);
                    break;
                case AccountTransaction account:
                    if (account.Source.Location != null
                        && selectedIds.Contains(account.Source.Location.Id.Value))
                    {
                        incoming += account.Amount;
                    }
                    decimal accountAmount = account.Destinations
                        .Where(destination => destination.Location != null
                            && selectedIds.Contains(destination.Location.Id.Value))
                        .Sum(destination => destination.Amount);
                    outgoing += accountAmount;
                    break;
                default:
                    break;
            }
        }
        return new LocationCashFlow(incoming, outgoing);
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
            or TransactionSort.Destination or TransactionSort.DestinationDescending
            or TransactionSort.FullyPosted or TransactionSort.FullyPostedDescending)
        {
            List<Transaction> allTransactions = await filtered.AsSplitQuery().ToListAsync(cancellationToken);
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
            transactions = await ApplySort(filtered, sort).AsSplitQuery()
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
        Transaction? transaction = await databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .SingleOrDefaultAsync(item => item.Id == transactionId, cancellationToken);
        if (transaction == null)
        {
            return null;
        }
        AccountingPeriod period = await databaseContext.AccountingPeriods.AsNoTracking()
            .SingleAsync(item => item.Id == transaction.AccountingPeriodId, cancellationToken);
        return new TransactionDetailsFacts(transaction, period);
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
        return new TransactionQueryFacts(new QueryPage<Transaction>(transactions, totalCount), periods);
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
                        || ((AccountTransaction)transaction).Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id))))
                || (transaction is RefundTransaction
                    && (((RefundTransaction)transaction).Sources.Any(source => source.Account != null && accountIds.Contains(source.Account.Id))
                        || accountIds.Contains(((RefundTransaction)transaction).Destination.Account.Id))));
        }
        if (filter.FundIds.Count > 0)
        {
            var fundIds = filter.FundIds.Select(id => new FundId(id)).ToList();
            transactions = transactions.Where(transaction =>
                (transaction is SpendingTransaction && ((SpendingTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
                || (transaction is IncomeTransaction && ((IncomeTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
                || (transaction is RefundTransaction && ((RefundTransaction)transaction).Sources.Any(source => source.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
                || (transaction is FundTransaction
                    && (fundIds.Contains(((FundTransaction)transaction).Source.Fund.Id)
                        || ((FundTransaction)transaction).Destinations.Any(destination => fundIds.Contains(destination.Fund.Id)))));
        }
        if (filter.LocationIds.Count > 0)
        {
            var locationIds = filter.LocationIds.Select(id => new LocationId(id)).ToList();
            transactions = transactions.Where(transaction =>
                (transaction is SpendingTransaction
                    && ((SpendingTransaction)transaction).Destinations.Any(destination =>
                        EF.Property<LocationId?>(destination, "LocationId") != null
                        && locationIds.Contains(EF.Property<LocationId?>(destination, "LocationId")!)))
                || (transaction is IncomeTransaction
                    && EF.Property<LocationId?>(((IncomeTransaction)transaction).Source, "LocationId") != null
                    && locationIds.Contains(EF.Property<LocationId?>(((IncomeTransaction)transaction).Source, "LocationId")!))
                || (transaction is RefundTransaction
                    && ((RefundTransaction)transaction).Sources.Any(source => EF.Property<LocationId?>(source, "LocationId") != null && locationIds.Contains(EF.Property<LocationId?>(source, "LocationId")!)))
                || (transaction is AccountTransaction
                    && ((EF.Property<LocationId?>(((AccountTransaction)transaction).Source, "LocationId") != null
                            && locationIds.Contains(EF.Property<LocationId?>(((AccountTransaction)transaction).Source, "LocationId")!))
                        || ((AccountTransaction)transaction).Destinations.Any(destination =>
                            EF.Property<LocationId?>(destination, "LocationId") != null
                            && locationIds.Contains(EF.Property<LocationId?>(destination, "LocationId")!)))));
        }
        if (filter.Types.Count > 0)
        {
            transactions = transactions.Where(transaction => filter.Types.Contains(transaction.Type));
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
            or TransactionSort.Destination or TransactionSort.DestinationDescending
            or TransactionSort.FullyPosted or TransactionSort.FullyPostedDescending =>
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
            TransactionSort.FullyPosted => transactions.OrderBy(IsFullyPosted).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
            TransactionSort.FullyPostedDescending => transactions.OrderByDescending(IsFullyPosted).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
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
        IncomeTransaction income => income.Source.Account?.Name ?? income.Source.Location?.Name,
        AccountTransaction account => account.Source.Account?.Name ?? account.Source.Location?.Name,
        FundTransaction fund => fund.Source.Fund.Name,
        RefundTransaction refund => string.Join(", ", refund.Sources.Select(source => source.Account?.Name ?? source.Location?.Name)),
        _ => null,
    };

    /// <summary>
    /// Gets the persisted destination display value for a Transaction.
    /// </summary>
    private static string GetDestination(Transaction transaction) => transaction switch
    {
        SpendingTransaction spending => string.Join(", ", spending.Destinations.Select(destination => destination.Account?.Name ?? destination.Location?.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        IncomeTransaction income => string.Join(", ", income.Destinations.Select(destination => destination.Account.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransaction account => string.Join(", ", account.Destinations.Select(destination => destination.Account?.Name ?? destination.Location?.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        FundTransaction fund => string.Join(", ", fund.Destinations.Select(destination => destination.Fund.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        RefundTransaction refund => refund.Destination.Account.Name,
        _ => string.Empty,
    };

    /// <summary>
    /// Determines whether a Transaction is posted to every affected Account.
    /// </summary>
    private static bool IsFullyPosted(Transaction transaction) => transaction.GetAllAffectedAccountIds()
        .All(accountId => transaction.GetPostedDateForAccount(accountId) != null);
}
