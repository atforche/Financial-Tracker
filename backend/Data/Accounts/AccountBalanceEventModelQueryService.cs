using Data.Transactions;
using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Accounts;
using Models.Transactions.Types;

namespace Data.Accounts;

/// <summary>
/// Read-only queries for Account balance-event API models not yet migrated to Domain.
/// </summary>
public sealed class AccountBalanceEventModelQueryService(
    DatabaseContext databaseContext,
    TransactionModelMapper transactionModelMapper) : TransactionReadQueryService(databaseContext, transactionModelMapper)
{
    /// <summary>
    /// Retrieves Account Balance Events in an Accounting Period range, or null when the range is invalid.
    /// </summary>
    public async Task<CollectionModel<AccountBalanceEventModel>?> GetAsync(
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
            DatabaseContext.Transactions.AsNoTracking().Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId)),
            cancellationToken);
        IEnumerable<AccountBalanceEventModel> events = transactions.SelectMany(transaction => transaction switch
        {
            SpendingTransactionModel spending => new[] { spending.Source.Account }.Concat(spending.Destinations.Where(destination => destination.Account != null).Select(destination => destination.Account!)),
            IncomeTransactionModel income => (income.Source.Account == null ? [] : new[] { income.Source.Account }).Concat(income.Destinations.Select(destination => destination.Account)),
            AccountTransactionModel account => (account.Source.Account == null ? [] : new[] { account.Source.Account }).Concat(account.Destinations.Where(destination => destination.Account != null).Select(destination => destination.Account!)),
            _ => [],
        });
        events = events.Where(balanceEvent => Matches(balanceEvent.Account, request.Filter));
        return ToCollection(Sort(events, request.Sort), request.Offset, request.Limit);
    }

    private static bool Matches(AccountModel account, AccountFilterModel? filter) =>
        (string.IsNullOrWhiteSpace(filter?.NameSearch) || account.Name.Contains(filter.NameSearch, StringComparison.OrdinalIgnoreCase))
        && (filter?.Names is not { Count: > 0 } names || names.Contains(account.Name))
        && (filter?.Types is not { Count: > 0 } types || types.Contains(account.Type));

    private static IOrderedEnumerable<AccountBalanceEventModel> Sort(IEnumerable<AccountBalanceEventModel> events, AccountBalanceEventSortModel? sort) => sort switch
    {
        AccountBalanceEventSortModel.AccountName => events.OrderBy(item => item.Account.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.AccountNameDescending => events.OrderByDescending(item => item.Account.Name).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.AccountingPeriodName => events.OrderBy(item => item.AccountingPeriod.Year).ThenBy(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.AccountingPeriodNameDescending => events.OrderByDescending(item => item.AccountingPeriod.Year).ThenByDescending(item => item.AccountingPeriod.Month).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.Date => events.OrderBy(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.Type => events.OrderBy(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.TypeDescending => events.OrderByDescending(item => item.Type).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.Amount => events.OrderBy(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.AmountDescending => events.OrderByDescending(item => item.Amount).ThenByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        AccountBalanceEventSortModel.DateDescending or null => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
        _ => events.OrderByDescending(item => item.Date).ThenBy(item => item.TransactionId),
    };
}