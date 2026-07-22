using Data.Transactions;
using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.FundPlans;
using Models.Transactions.Types;

namespace Data.FundPlans;

/// <summary>
/// Read-only queries for Fund Plan balance-event API models.
/// </summary>
public sealed class FundPlanBalanceEventQueryService(
    DatabaseContext databaseContext,
    TransactionModelMapper transactionModelMapper) : TransactionReadQueryService(databaseContext, transactionModelMapper)
{
    /// <summary>
    /// Retrieves Fund Plan balance events in a date range.
    /// </summary>
    public async Task<CollectionModel<FundPlanBalanceEventModel>> GetAsync(
        FundPlanBalanceEventsInDateRangeQueryParameterModel request,
        CancellationToken cancellationToken = default) =>
        ToCollection(Sort(Filter(await GetTransactionsAsync(
            DatabaseContext.Transactions.AsNoTracking().Where(transaction => transaction.Date >= request.Range.Start && transaction.Date <= request.Range.End),
            cancellationToken), request.Filter), request.Sort), request.Offset, request.Limit);

    /// <summary>
    /// Retrieves Fund Plan balance events in an Accounting Period range.
    /// </summary>
    public async Task<CollectionModel<FundPlanBalanceEventModel>?> GetAsync(
        FundPlanBalanceEventsInAccountingPeriodRangeQueryParameterModel request,
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

    private static IEnumerable<FundPlanBalanceEventModel> Filter(IEnumerable<TransactionModel> transactions, FundPlanFilterModel? filter) =>
        transactions.SelectMany(transaction => transaction switch
        {
            SpendingTransactionModel spending => spending.Destinations.SelectMany(destination => destination.FundPlans),
            IncomeTransactionModel income => income.Destinations.SelectMany(destination => destination.FundPlans),
            FundTransactionModel fund => (fund.Source.FundPlan == null ? [] : new[] { fund.Source.FundPlan })
                .Concat(fund.Destinations.Where(destination => destination.FundPlan != null).Select(destination => destination.FundPlan!)),
            _ => [],
        }).Where(balanceEvent => filter?.FundIds is not { Count: > 0 } fundIds || fundIds.Contains(balanceEvent.Fund.Id));

    private static IOrderedEnumerable<FundPlanBalanceEventModel> Sort(IEnumerable<FundPlanBalanceEventModel> events, FundPlanBalanceEventSortModel? sort) => sort switch
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
}