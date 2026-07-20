using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Accounts;
using Models.BalanceEvents;
using Models.FundPlans;
using Models.Funds;
using Models.Transactions.Types;

namespace Data.BalanceEvents;

/// <summary>
/// Read-only queries for Balance Event API models.
/// </summary>
public sealed class BalanceEventQueryService(DatabaseContext databaseContext, TransactionModelMapper transactionModelMapper)
{
    /// <summary>
    /// Retrieves Account Balance Events in a date range.
    /// </summary>
    public async Task<CollectionModel<AccountBalanceEventModel>> GetAccountEventsAsync(
        AccountBalanceEventsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End),
            cancellationToken);
        return ToCollection(ApplySort(GetAccountEvents(transactions).Where(balanceEvent => Matches(balanceEvent.Account, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves Account Balance Events in an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<CollectionModel<AccountBalanceEventModel>?> GetAccountEventsAsync(
        AccountBalanceEventsInAccountingPeriodRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Guid>? periodIds = await GetAccountingPeriodIdsAsync(request.Range, cancellationToken);
        if (periodIds == null)
        {
            return null;
        }
        var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)),
            cancellationToken);
        return ToCollection(ApplySort(GetAccountEvents(transactions).Where(balanceEvent => Matches(balanceEvent.Account, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves Fund Balance Events in a date range.
    /// </summary>
    public async Task<CollectionModel<FundBalanceEventModel>> GetFundEventsAsync(
        FundBalanceEventsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End),
            cancellationToken);
        return ToCollection(ApplySort(GetFundEvents(transactions).Where(balanceEvent => Matches(balanceEvent.Fund, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves Fund Balance Events in an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<CollectionModel<FundBalanceEventModel>?> GetFundEventsAsync(
        FundBalanceEventsInAccountingPeriodRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Guid>? periodIds = await GetAccountingPeriodIdsAsync(request.Range, cancellationToken);
        if (periodIds == null)
        {
            return null;
        }
        var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)),
            cancellationToken);
        return ToCollection(ApplySort(GetFundEvents(transactions).Where(balanceEvent => Matches(balanceEvent.Fund, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves Fund Plan balance events in a date range.
    /// </summary>
    public async Task<CollectionModel<FundPlanBalanceEventModel>> GetFundPlanEventsAsync(
        FundPlanBalanceEventsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End),
            cancellationToken);
        return ToCollection(ApplySort(GetFundPlanEvents(transactions).Where(item => Matches(item, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves Fund Plan balance events in an Accounting Period range.
    /// </summary>
    public async Task<CollectionModel<FundPlanBalanceEventModel>?> GetFundPlanEventsAsync(
        FundPlanBalanceEventsInAccountingPeriodRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Guid>? periodIds = await GetAccountingPeriodIdsAsync(request.Range, cancellationToken);
        if (periodIds == null)
        {
            return null;
        }
        var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
        IReadOnlyCollection<TransactionModel> transactions = await GetTransactionsAsync(
            databaseContext.Transactions.AsNoTracking().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)),
            cancellationToken);
        return ToCollection(ApplySort(GetFundPlanEvents(transactions).Where(item => Matches(item, request.Filter)), request.Sort), request.Offset, request.Limit);
    }

    /// <summary>
    /// Retrieves and maps the provided Transactions.
    /// </summary>
    private async Task<IReadOnlyCollection<TransactionModel>> GetTransactionsAsync(IQueryable<Transaction> query, CancellationToken cancellationToken)
    {
        List<Transaction> transactions = await query.ToListAsync(cancellationToken);
        return await transactionModelMapper.MapAsync(transactions, cancellationToken);
    }

    /// <summary>
    /// Resolves the Accounting Period IDs in the provided range.
    /// </summary>
    private async Task<IReadOnlyCollection<Guid>?> GetAccountingPeriodIdsAsync(AccountingPeriodRangeModel range, CancellationToken cancellationToken)
    {
        var startId = new AccountingPeriodId(range.Start);
        var endId = new AccountingPeriodId(range.End);
        List<AccountingPeriod> endpoints = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => period.Id == startId || period.Id == endId).ToListAsync(cancellationToken);
        AccountingPeriod? start = endpoints.SingleOrDefault(period => period.Id.Value == range.Start);
        AccountingPeriod? end = endpoints.SingleOrDefault(period => period.Id.Value == range.End);
        if (start == null || end == null || (start.Year * 12) + start.Month > (end.Year * 12) + end.Month)
        {
            return null;
        }
        int startIndex = (start.Year * 12) + start.Month;
        int endIndex = (end.Year * 12) + end.Month;
        List<AccountingPeriodId> periodIds = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => (period.Year * 12) + period.Month >= startIndex && (period.Year * 12) + period.Month <= endIndex)
            .Select(period => period.Id).ToListAsync(cancellationToken);
        return periodIds.Count == endIndex - startIndex + 1 ? periodIds.Select(id => id.Value).ToList() : null;
    }

    /// <summary>
    /// Gets all Account Balance Events from the provided Transactions.
    /// </summary>
    private static IEnumerable<AccountBalanceEventModel> GetAccountEvents(IEnumerable<TransactionModel> transactions) => transactions.SelectMany(transaction => transaction switch
    {
        SpendingTransactionModel spending => new[] { spending.Source.Account }.Concat(spending.Destinations.Where(destination => destination.Account != null).Select(destination => destination.Account!)),
        IncomeTransactionModel income => (income.Source.Account == null ? Enumerable.Empty<AccountBalanceEventModel>() : new[] { income.Source.Account! }).Concat(income.Destinations.Select(destination => destination.Account)),
        AccountTransactionModel account => (account.Source.Account == null ? Enumerable.Empty<AccountBalanceEventModel>() : new[] { account.Source.Account! }).Concat(account.Destinations.Where(destination => destination.Account != null).Select(destination => destination.Account!)),
        _ => Enumerable.Empty<AccountBalanceEventModel>(),
    });

    /// <summary>
    /// Gets all Fund Balance Events from the provided Transactions.
    /// </summary>
    private static IEnumerable<FundBalanceEventModel> GetFundEvents(IEnumerable<TransactionModel> transactions) => transactions.SelectMany(transaction => transaction switch
    {
        SpendingTransactionModel spending => spending.Destinations.SelectMany(destination => destination.FundAssignments),
        IncomeTransactionModel income => income.Destinations.SelectMany(destination => destination.FundAssignments),
        FundTransactionModel fund => new[] { fund.Source.Fund }.Concat(fund.Destinations.Select(destination => destination.Fund)),
        _ => Enumerable.Empty<FundBalanceEventModel>(),
    });

    /// <summary>
    /// Gets all Fund Plan balance events from the provided Transactions.
    /// </summary>
    private static IEnumerable<FundPlanBalanceEventModel> GetFundPlanEvents(IEnumerable<TransactionModel> transactions) => transactions.SelectMany(transaction => transaction switch
    {
        SpendingTransactionModel spending => spending.Destinations.SelectMany(destination => destination.FundPlans),
        IncomeTransactionModel income => income.Destinations.SelectMany(destination => destination.FundPlans),
        FundTransactionModel fund => (fund.Source.FundPlan == null ? [] : new[] { fund.Source.FundPlan })
            .Concat(fund.Destinations.Where(destination => destination.FundPlan != null).Select(destination => destination.FundPlan!)),
        _ => [],
    });

    /// <summary>
    /// Determines whether the provided Account matches the provided filter.
    /// </summary>
    private static bool Matches(AccountModel account, AccountFilterModel? filter) =>
        (string.IsNullOrWhiteSpace(filter?.NameSearch) || account.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase)) &&
        (filter?.Names is not { Count: > 0 } names || names.Contains(account.Name)) &&
        (filter?.Types is not { Count: > 0 } types || types.Contains(account.Type));

    /// <summary>
    /// Determines whether the provided Fund matches the provided filter.
    /// </summary>
    private static bool Matches(FundModel fund, FundFilterModel? filter) =>
        (string.IsNullOrWhiteSpace(filter?.NameSearch) || fund.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase)) &&
        (filter?.Names is not { Count: > 0 } names || names.Contains(fund.Name));

    /// <summary>
    /// Determines whether a Fund Plan event matches the provided filter.
    /// </summary>
    private static bool Matches(FundPlanBalanceEventModel balanceEvent, FundPlanFilterModel? filter) =>
        filter?.FundIds is not { Count: > 0 } fundIds || fundIds.Contains(balanceEvent.Fund.Id);

    /// <summary>
    /// Applies the provided sort to the provided Account Balance Events, or a default sort when the provided sort is null or invalid.
    /// </summary>
    private static IOrderedEnumerable<AccountBalanceEventModel> ApplySort(IEnumerable<AccountBalanceEventModel> events, AccountBalanceEventSortModel? sort) => sort switch
    {
        AccountBalanceEventSortModel.AccountName => events.OrderBy(balanceEvent => balanceEvent.Account.Name).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.AccountNameDescending => events.OrderByDescending(balanceEvent => balanceEvent.Account.Name).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.AccountingPeriodName => events.OrderBy(balanceEvent => balanceEvent.AccountingPeriod.Year).ThenBy(balanceEvent => balanceEvent.AccountingPeriod.Month).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.AccountingPeriodNameDescending => events.OrderByDescending(balanceEvent => balanceEvent.AccountingPeriod.Year).ThenByDescending(balanceEvent => balanceEvent.AccountingPeriod.Month).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.Date => events.OrderBy(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.DateDescending => events.OrderByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.Type => events.OrderBy(balanceEvent => balanceEvent.Type).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.TypeDescending => events.OrderByDescending(balanceEvent => balanceEvent.Type).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.Amount => events.OrderBy(balanceEvent => balanceEvent.Amount).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        AccountBalanceEventSortModel.AmountDescending => events.OrderByDescending(balanceEvent => balanceEvent.Amount).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        _ => events.OrderByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
    };

    /// <summary>
    /// Applies the provided sort to the provided Fund Balance Events, or a default sort when the provided sort is null or invalid.
    /// </summary>
    private static IOrderedEnumerable<FundBalanceEventModel> ApplySort(IEnumerable<FundBalanceEventModel> events, FundBalanceEventSortModel? sort) => sort switch
    {
        FundBalanceEventSortModel.FundName => events.OrderBy(balanceEvent => balanceEvent.Fund.Name).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.FundNameDescending => events.OrderByDescending(balanceEvent => balanceEvent.Fund.Name).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.AccountingPeriodName => events.OrderBy(balanceEvent => balanceEvent.AccountingPeriod.Year).ThenBy(balanceEvent => balanceEvent.AccountingPeriod.Month).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.AccountingPeriodNameDescending => events.OrderByDescending(balanceEvent => balanceEvent.AccountingPeriod.Year).ThenByDescending(balanceEvent => balanceEvent.AccountingPeriod.Month).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.Date => events.OrderBy(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.DateDescending => events.OrderByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.Type => events.OrderBy(balanceEvent => balanceEvent.Type).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.TypeDescending => events.OrderByDescending(balanceEvent => balanceEvent.Type).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.Amount => events.OrderBy(balanceEvent => balanceEvent.Amount).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        FundBalanceEventSortModel.AmountDescending => events.OrderByDescending(balanceEvent => balanceEvent.Amount).ThenByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
        _ => events.OrderByDescending(balanceEvent => balanceEvent.Date).ThenBy(balanceEvent => balanceEvent.TransactionId),
    };

    /// <summary>
    /// Applies ordering to Fund Plan balance events.
    /// </summary>
    private static IOrderedEnumerable<FundPlanBalanceEventModel> ApplySort(
        IEnumerable<FundPlanBalanceEventModel> events,
        FundPlanBalanceEventSortModel? sort) => sort switch
        {
            FundPlanBalanceEventSortModel.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.Date => events.OrderBy(item => item.Date).ThenBy(item => item.TransactionId),
            FundPlanBalanceEventSortModel.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.Date),
            FundPlanBalanceEventSortModel.DateDescending or null => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
            _ => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        };

    /// <summary>
    /// Projects the provided items to a CollectionModel, applying the provided offset and limit for pagination, and including the total count of items before pagination.
    /// </summary>
    private static CollectionModel<T> ToCollection<T>(IEnumerable<T> items, int? offset, int? limit)
    {
        var allItems = items.ToList();
        return new CollectionModel<T>
        {
            Items = allItems.Skip(offset ?? 0).Take(limit ?? int.MaxValue).ToList(),
            TotalCount = allItems.Count,
        };
    }
}