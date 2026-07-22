using Domain.AccountingPeriods;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Transactions.Types;

namespace Data.Transactions;

/// <summary>
/// Provides shared transaction fact retrieval for entity-owned read services.
/// </summary>
public abstract class TransactionReadQueryService(
    DatabaseContext databaseContext,
    TransactionModelMapper transactionModelMapper)
{
    /// <summary>
    /// Gets the database context used by the read service.
    /// </summary>
    protected DatabaseContext DatabaseContext { get; } = databaseContext;

    /// <summary>
    /// Retrieves and maps the provided Transactions.
    /// </summary>
    protected async Task<IReadOnlyCollection<TransactionModel>> GetTransactionsAsync(
        IQueryable<Transaction> query,
        CancellationToken cancellationToken)
    {
        List<Transaction> transactions = await query.ToListAsync(cancellationToken);
        return await transactionModelMapper.MapAsync(transactions, cancellationToken);
    }

    /// <summary>
    /// Resolves the Accounting Period IDs in the provided range.
    /// </summary>
    protected async Task<IReadOnlyCollection<Guid>?> GetAccountingPeriodIdsAsync(
        AccountingPeriodRangeModel range,
        CancellationToken cancellationToken)
    {
        var startId = new AccountingPeriodId(range.Start);
        var endId = new AccountingPeriodId(range.End);
        List<AccountingPeriod> endpoints = await DatabaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => period.Id == startId || period.Id == endId).ToListAsync(cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == range.Start);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == range.End);
        if (start == null || end == null || (start.Year * 12) + start.Month > (end.Year * 12) + end.Month)
        {
            return null;
        }
        int startIndex = (start.Year * 12) + start.Month;
        int endIndex = (end.Year * 12) + end.Month;
        List<AccountingPeriodId> periodIds = await DatabaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => (period.Year * 12) + period.Month >= startIndex && (period.Year * 12) + period.Month <= endIndex)
            .Select(period => period.Id).ToListAsync(cancellationToken);
        return periodIds.Count == endIndex - startIndex + 1 ? periodIds.Select(id => id.Value).ToList() : null;
    }

    /// <summary>
    /// Projects the provided items to a CollectionModel.
    /// </summary>
    protected static CollectionModel<T> ToCollection<T>(IEnumerable<T> items, int? offset, int? limit)
    {
        var allItems = items.ToList();
        return new CollectionModel<T>
        {
            Items = allItems.Skip(offset ?? 0).Take(limit ?? int.MaxValue).ToList(),
            TotalCount = allItems.Count,
        };
    }
}