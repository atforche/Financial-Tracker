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
    public async Task<TransactionQueryFacts> GetAsync(
        TransactionQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> filtered = ApplyFilter(databaseContext.Transactions.AsNoTracking(), query.Filter);
        int totalCount = await filtered.CountAsync(cancellationToken);
        IReadOnlyCollection<Transaction> transactions;
        if (query.Sort is TransactionSort.AccountingPeriod or TransactionSort.AccountingPeriodDescending
            or TransactionSort.Source or TransactionSort.SourceDescending
            or TransactionSort.Destination or TransactionSort.DestinationDescending)
        {
            List<Transaction> allTransactions = await filtered.ToListAsync(cancellationToken);
            Dictionary<AccountingPeriodId, int> periodOrder = query.Sort is TransactionSort.AccountingPeriod or TransactionSort.AccountingPeriodDescending
                ? await databaseContext.AccountingPeriods.AsNoTracking()
                    .ToDictionaryAsync(period => period.Id, period => (period.Year * 12) + period.Month, cancellationToken)
                : [];
            transactions = Sort(allTransactions, query.Sort, periodOrder)
                .Skip(query.Offset)
                .Take(query.Limit ?? int.MaxValue)
                .ToList();
        }
        else
        {
            transactions = await ApplySort(filtered, query.Sort)
                .Skip(query.Offset)
                .Take(query.Limit ?? int.MaxValue)
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