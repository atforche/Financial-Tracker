using Data.Transactions;
using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Funds;
using Models.Transactions.Types;

namespace Data.Funds;

/// <summary>
/// Read-only queries for Fund balance-event API models.
/// </summary>
public sealed class FundBalanceEventQueryService(
    DatabaseContext databaseContext,
    TransactionModelMapper transactionModelMapper) : TransactionReadQueryService(databaseContext, transactionModelMapper)
{
    /// <summary>
    /// Retrieves Fund Balance Events in a date range.
    /// </summary>
    public async Task<CollectionModel<FundBalanceEventModel>> GetAsync(
        FundBalanceEventsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default) =>
        ToCollection(Sort(Filter(await GetTransactionsAsync(
            DatabaseContext.Transactions.AsNoTracking().Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End),
            cancellationToken), request.Filter), request.Sort), request.Offset, request.Limit);

    /// <summary>
    /// Retrieves Fund Balance Events in an Accounting Period range.
    /// </summary>
    public async Task<CollectionModel<FundBalanceEventModel>?> GetAsync(
        FundBalanceEventsInAccountingPeriodRangeQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Guid>? periodIds = await GetAccountingPeriodIdsAsync(request.Range, cancellationToken);
        if (periodIds == null)
        {
            return null;
        }
        var accountingPeriodIds = periodIds.Select(id => new AccountingPeriodId(id)).ToList();
        return ToCollection(Sort(Filter(await GetTransactionsAsync(
            DatabaseContext.Transactions.AsNoTracking().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)),
            cancellationToken), request.Filter), request.Sort), request.Offset, request.Limit);
    }

    private static IEnumerable<FundBalanceEventModel> Filter(IEnumerable<TransactionModel> transactions, FundFilterModel? filter) =>
        transactions.SelectMany(transaction => transaction switch
        {
            SpendingTransactionModel spending => spending.Destinations.SelectMany(destination => destination.FundAssignments),
            IncomeTransactionModel income => income.Destinations.SelectMany(destination => destination.FundAssignments),
            FundTransactionModel fund => new[] { fund.Source.Fund }.Concat(fund.Destinations.Select(destination => destination.Fund)),
            _ => [],
        }).Where(balanceEvent =>
            (string.IsNullOrWhiteSpace(filter?.NameSearch) || balanceEvent.Fund.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase))
            && (filter?.Names is not { Count: > 0 } names || names.Contains(balanceEvent.Fund.Name)));

    private static IOrderedEnumerable<FundBalanceEventModel> Sort(IEnumerable<FundBalanceEventModel> events, FundBalanceEventSortModel? sort) => sort switch
    {
        FundBalanceEventSortModel.FundName => events.OrderBy(item => item.Fund.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.FundNameDescending => events.OrderByDescending(item => item.Fund.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.AccountingPeriodName => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.AccountingPeriodNameDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.Date => events.OrderBy(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        FundBalanceEventSortModel.DateDescending or null => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        _ => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
    };
}