using Domain.AccountingPeriods;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Transactions;
using Models.Transactions.Types;

namespace Data.Transactions;

/// <summary>
/// Read-only queries for Transaction API models.
/// </summary>
public sealed class TransactionQueryService(DatabaseContext databaseContext, TransactionModelMapper transactionModelMapper)
{
    /// <summary>
    /// Retrieves Transactions matching the provided query.
    /// </summary>
    public async Task<CollectionModel<TransactionModel>> GetAsync(
        TransactionQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> query = ApplyFilter(databaseContext.Transactions.AsNoTracking(), request.Filter);
        int totalCount = await query.CountAsync(cancellationToken);
        IReadOnlyCollection<TransactionModel> items = await GetPageAsync(query, request.Sort, request.Offset, request.Limit, cancellationToken);
        return new CollectionModel<TransactionModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves Transactions for an Accounting Period.
    /// </summary>
    public Task<CollectionModel<TransactionModel>> GetForAccountingPeriodAsync(
        Guid accountingPeriodId,
        Models.AccountingPeriods.AccountingPeriodWithTransactionsQueryParameterModel request,
        CancellationToken cancellationToken = default) =>
        GetAsync(new TransactionQueryParameterModel
        {
            Filter = new TransactionFilterModel { AccountingPeriodIds = [accountingPeriodId] },
            Sort = request.Sort,
            Limit = request.Limit,
            Offset = request.Offset,
        }, cancellationToken);

    /// <summary>
    /// Retrieves Transactions in a date range.
    /// </summary>
    public async Task<TransactionsInDateRangeModel> GetInDateRangeAsync(
        TransactionsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Transaction> query = ApplyFilter(databaseContext.Transactions.AsNoTracking(), request.Filter)
            .Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End);
        RangeContent content = await ExecuteRangeAsync(query, request.Sort, request.Offset, request.Limit, cancellationToken);
        return new TransactionsInDateRangeModel
        {
            Transactions = content.Transactions,
            AvailableAccountNames = content.AccountNames,
            AvailableFundNames = content.FundNames,
            TransactionTypes = content.TransactionTypes,
            Limit = request.Limit,
            Offset = request.Offset,
        };
    }

    /// <summary>
    /// Retrieves Transactions in an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<TransactionsInAccountingPeriodRangeModel?> GetInAccountingPeriodRangeAsync(
        TransactionsInAccountingPeriodRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        var startId = new AccountingPeriodId(request.Range.Start);
        var endId = new AccountingPeriodId(request.Range.End);
        List<AccountingPeriod> endpoints = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => period.Id == startId || period.Id == endId).ToListAsync(cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == request.Range.Start);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == request.Range.End);
        if (start == null || end == null || (start.Year * 12) + start.Month > (end.Year * 12) + end.Month)
        {
            return null;
        }
        int startIndex = (start.Year * 12) + start.Month;
        int endIndex = (end.Year * 12) + end.Month;
        List<AccountingPeriodId> periodIds = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => (period.Year * 12) + period.Month >= startIndex && (period.Year * 12) + period.Month <= endIndex)
            .Select(period => period.Id).ToListAsync(cancellationToken);
        if (periodIds.Count != endIndex - startIndex + 1)
        {
            return null;
        }
        IQueryable<Transaction> query = ApplyFilter(databaseContext.Transactions.AsNoTracking(), request.Filter)
            .Where(transaction => periodIds.Contains(transaction.AccountingPeriodId));
        RangeContent content = await ExecuteRangeAsync(query, request.Sort, request.Offset, request.Limit, cancellationToken);
        return new TransactionsInAccountingPeriodRangeModel
        {
            Transactions = content.Transactions,
            AvailableAccountNames = content.AccountNames,
            AvailableFundNames = content.FundNames,
            TransactionTypes = content.TransactionTypes,
            Limit = request.Limit,
            Offset = request.Offset,
        };
    }

    /// <summary>
    /// Executes the query with the specified range parameters and returns the content.
    /// </summary>
    private async Task<RangeContent> ExecuteRangeAsync(
        IQueryable<Transaction> filteredQuery,
        TransactionSortModel? sort,
        int? offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        int totalCount = await filteredQuery.CountAsync(cancellationToken);
        List<TransactionSummaryByTypeModel> summaries = await filteredQuery.GroupBy(transaction => transaction.Type)
            .Select(group => new TransactionSummaryByTypeModel
            {
                TransactionType = (TransactionTypeModel)group.Key,
                TotalCount = group.Count(),
                TotalAmount = group.Sum(transaction => transaction.Amount),
            }).ToListAsync(cancellationToken);
        IReadOnlyCollection<TransactionModel> models = await GetPageAsync(filteredQuery, sort, offset, limit, cancellationToken);
        List<string> accountNames = await databaseContext.Accounts.AsNoTracking().OrderBy(account => account.Name).Select(account => account.Name).ToListAsync(cancellationToken);
        List<string> fundNames = await databaseContext.Funds.AsNoTracking().OrderBy(fund => fund.Name).Select(fund => fund.Name).ToListAsync(cancellationToken);
        return new RangeContent(
            new CollectionModel<TransactionModel> { Items = models, TotalCount = totalCount },
            accountNames,
            fundNames,
            summaries);
    }

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<Transaction> ApplyFilter(IQueryable<Transaction> query, TransactionFilterModel? filter)
    {
        if (filter?.AccountingPeriodIds is { Count: > 0 } periodIds)
        {
            var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
            query = query.Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId));
        }
        if (filter?.AccountIds is { Count: > 0 } accountIds)
        {
            var domainAccountIds = accountIds.Select(id => new Domain.Accounts.AccountId(id)).ToList();
            query = query.Where(transaction =>
                (transaction is SpendingTransaction &&
                    (domainAccountIds.Contains(((SpendingTransaction)transaction).Source.Account.Id) ||
                     ((SpendingTransaction)transaction).Destinations.Any(destination => destination.Account != null && domainAccountIds.Contains(destination.Account.Id)))) ||
                (transaction is IncomeTransaction &&
                    ((((IncomeTransaction)transaction).Source.Account != null && domainAccountIds.Contains(((IncomeTransaction)transaction).Source.Account!.Id)) ||
                     ((IncomeTransaction)transaction).Destinations.Any(destination => domainAccountIds.Contains(destination.Account.Id)))) ||
                (transaction is AccountTransaction &&
                    ((((AccountTransaction)transaction).Source.Account != null && domainAccountIds.Contains(((AccountTransaction)transaction).Source.Account!.Id)) ||
                     ((AccountTransaction)transaction).Destinations.Any(destination => destination.Account != null && domainAccountIds.Contains(destination.Account.Id)))));
        }
        if (filter?.FundIds is { Count: > 0 } fundIds)
        {
            var domainFundIds = fundIds.Select(id => new Domain.Funds.FundId(id)).ToList();
            query = query.Where(transaction =>
                (transaction is SpendingTransaction && ((SpendingTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(assignment => domainFundIds.Contains(assignment.FundId)))) ||
                (transaction is IncomeTransaction && ((IncomeTransaction)transaction).Destinations.Any(destination => destination.FundAssignments.Any(assignment => domainFundIds.Contains(assignment.FundId)))) ||
                (transaction is FundTransaction &&
                    (domainFundIds.Contains(((FundTransaction)transaction).Source.Fund.Id) ||
                     ((FundTransaction)transaction).Destinations.Any(destination => domainFundIds.Contains(destination.Fund.Id)))));
        }
        return query;
    }

    /// <summary>
    /// Applies the sort to the provided query
    /// </summary>
    private static IQueryable<Transaction> ApplySort(IQueryable<Transaction> query, TransactionSortModel? sort) => sort switch
    {
        TransactionSortModel.Date => query.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.DateDescending => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.Description => query.OrderBy(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.DescriptionDescending => query.OrderByDescending(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.Amount => query.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.AmountDescending => query.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.AccountingPeriod => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.AccountingPeriodDescending => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.Source => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.SourceDescending => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.Destination => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        TransactionSortModel.DestinationDescending => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
        _ => query.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ThenBy(transaction => transaction.Id),
    };

    /// <summary>
    /// Gets the page of transactions from the provided query
    /// </summary>
    private async Task<IReadOnlyCollection<TransactionModel>> GetPageAsync(
        IQueryable<Transaction> query,
        TransactionSortModel? sort,
        int? offset,
        int? limit,
        CancellationToken cancellationToken)
    {
        if (sort is not (TransactionSortModel.AccountingPeriod or TransactionSortModel.AccountingPeriodDescending
            or TransactionSortModel.Source or TransactionSortModel.SourceDescending
            or TransactionSortModel.Destination or TransactionSortModel.DestinationDescending))
        {
            List<Transaction> page = await ApplySort(query, sort).Skip(offset ?? 0).Take(limit ?? int.MaxValue).ToListAsync(cancellationToken);
            return await transactionModelMapper.MapAsync(page, cancellationToken);
        }

        List<Transaction> transactions = await query.ToListAsync(cancellationToken);
        IReadOnlyCollection<TransactionModel> models = await transactionModelMapper.MapAsync(transactions, cancellationToken);
        Dictionary<Guid, int> periodOrder = sort is TransactionSortModel.AccountingPeriod or TransactionSortModel.AccountingPeriodDescending
            ? await databaseContext.AccountingPeriods.AsNoTracking()
                .ToDictionaryAsync(period => period.Id.Value, period => (period.Year * 12) + period.Month, cancellationToken)
            : [];
        IEnumerable<TransactionModel> sorted = sort switch
        {
            TransactionSortModel.AccountingPeriod => models.OrderBy(model => periodOrder[model.AccountingPeriodId]).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.AccountingPeriodDescending => models.OrderByDescending(model => periodOrder[model.AccountingPeriodId]).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.Source => models.OrderBy(GetSource).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.SourceDescending => models.OrderByDescending(GetSource).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.Destination => models.OrderBy(GetDestination).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.DestinationDescending => models.OrderByDescending(GetDestination).ThenByDescending(model => model.Date).ThenByDescending(model => model.Sequence).ThenBy(model => model.Id),
            TransactionSortModel.Date => models,
            TransactionSortModel.DateDescending => models,
            TransactionSortModel.Description => models,
            TransactionSortModel.DescriptionDescending => models,
            TransactionSortModel.Amount => models,
            TransactionSortModel.AmountDescending => models,
            _ => models,
        };
        return sorted.Skip(offset ?? 0).Take(limit ?? int.MaxValue).ToList();
    }

    /// <summary>
    /// Gets the source string for the provided transaction
    /// </summary>
    private static string? GetSource(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spending => spending.Source.Account.Account.Name,
        IncomeTransactionModel income => income.Source.Account?.Account.Name ?? income.Source.Location,
        AccountTransactionModel account => account.Source.Account?.Account.Name ?? account.Source.Location,
        FundTransactionModel fund => fund.Source.Fund.Fund.Name,
        _ => null,
    };

    /// <summary>
    /// Gets the destination string for the provided transaction
    /// </summary>
    private static string GetDestination(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spending => string.Join(", ", spending.Destinations.Select(destination => destination.Account?.Account.Name ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        IncomeTransactionModel income => string.Join(", ", income.Destinations.Select(destination => destination.Account.Account.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransactionModel account => string.Join(", ", account.Destinations.Select(destination => destination.Account?.Account.Name ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        FundTransactionModel fund => string.Join(", ", fund.Destinations.Select(destination => destination.Fund.Fund.Name).Distinct(StringComparer.OrdinalIgnoreCase)),
        _ => string.Empty,
    };

    /// <summary>
    /// Record representing information about objects in a range
    /// </summary>
    private sealed record RangeContent(
        CollectionModel<TransactionModel> Transactions,
        IReadOnlyCollection<string> AccountNames,
        IReadOnlyCollection<string> FundNames,
        IReadOnlyCollection<TransactionSummaryByTypeModel> TransactionTypes);
}