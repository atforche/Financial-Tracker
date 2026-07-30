using Domain;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.BalanceEvents;
using Models;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.BalanceEvents;

namespace Rest.Accounts;

/// <summary>
/// Converts Account balance-event API models and Domain query results.
/// </summary>
public sealed class AccountBalanceEventConverter
{
    /// <summary>
    /// Converts the provided API Account balance-event query to a Domain query.
    /// </summary>
    public AccountBalanceEventAccountQuery ToDomain(
        Guid accountId,
        AccountBalanceEventsQueryParameterModel model) => new(
            accountId,
            model.Range.Start,
            model.Range.End,
            ToDomain(model.Sort),
            model.Offset ?? 0,
            model.Limit);

    /// <summary>
    /// Converts the provided API query to a Domain query.
    /// </summary>
    public AccountBalanceEventQuery ToDomain(AccountBalanceEventsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Accounting Period range API query to a Domain query.
    /// </summary>
    public AccountBalanceEventAccountingPeriodRangeQuery ToDomain(AccountBalanceEventsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided API Account filter to a Domain filter.
    /// </summary>
    private static AccountFilter ToDomain(AccountFilterModel? filter) => new(
        filter?.NameSearch,
        filter?.Names ?? [],
        filter?.Types?.Select(type => (AccountType)type).ToList() ?? []);

    /// <summary>
    /// Converts the provided API sort to a Domain sort.
    /// </summary>
    private static AccountBalanceEventSort ToDomain(AccountBalanceEventSortModel? sort) => sort switch
    {
        AccountBalanceEventSortModel.AccountName => AccountBalanceEventSort.AccountName,
        AccountBalanceEventSortModel.AccountNameDescending => AccountBalanceEventSort.AccountNameDescending,
        AccountBalanceEventSortModel.AccountingPeriodName => AccountBalanceEventSort.AccountingPeriod,
        AccountBalanceEventSortModel.AccountingPeriodNameDescending => AccountBalanceEventSort.AccountingPeriodDescending,
        AccountBalanceEventSortModel.Date => AccountBalanceEventSort.Date,
        AccountBalanceEventSortModel.DateDescending => AccountBalanceEventSort.DateDescending,
        AccountBalanceEventSortModel.Type => AccountBalanceEventSort.Type,
        AccountBalanceEventSortModel.TypeDescending => AccountBalanceEventSort.TypeDescending,
        AccountBalanceEventSortModel.Amount => AccountBalanceEventSort.Amount,
        AccountBalanceEventSortModel.AmountDescending => AccountBalanceEventSort.AmountDescending,
        AccountBalanceEventSortModel.Counterparty => AccountBalanceEventSort.Counterparty,
        AccountBalanceEventSortModel.CounterpartyDescending => AccountBalanceEventSort.CounterpartyDescending,
        _ => AccountBalanceEventSort.DateDescending,
    };

    /// <summary>
    /// Converts the provided Domain page to an API collection.
    /// </summary>
    public CollectionModel<AccountBalanceEventModel> ToModel(QueryPage<AccountBalanceEvent> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Domain Account balance event to an API model.
    /// </summary>
    public AccountBalanceEventModel ToModel(AccountBalanceEvent balanceEvent) => new()
    {
        AccountingPeriod = new AccountingPeriodModel
        {
            Id = balanceEvent.AccountingPeriod.Id.Value,
            Name = balanceEvent.AccountingPeriod.Name,
            Year = balanceEvent.AccountingPeriod.Year,
            Month = balanceEvent.AccountingPeriod.Month,
            IsOpen = balanceEvent.AccountingPeriod.IsOpen,
        },
        TransactionId = balanceEvent.TransactionId.Value,
        TransactionDate = balanceEvent.TransactionDate,
        TransactionSequence = balanceEvent.TransactionSequence,
        EventDate = balanceEvent.EventDate,
        EventDateSequence = balanceEvent.EventDateSequence,
        Type = balanceEvent.Type == BalanceEventType.Debit ? BalanceEventTypeModel.Debit : BalanceEventTypeModel.Credit,
        IsPosted = balanceEvent.IsPosted,
        Amount = balanceEvent.Amount,
        Account = new AccountModel
        {
            Id = balanceEvent.Account.Id.Value,
            Name = balanceEvent.Account.Name,
            Type = AccountTypeConverter.ToModel(balanceEvent.Account.Type),
        },
        Source = ToModel(balanceEvent.Source),
        Destinations = balanceEvent.Destinations.Select(ToModel).ToList(),
        PreviousBalance = ToModel(balanceEvent.PreviousBalance),
        NewBalance = ToModel(balanceEvent.NewBalance),
    };

    /// <summary>
    /// Converts a Domain Account balance event party to an API model.
    /// </summary>
    private static AccountBalanceEventPartyModel ToModel(AccountBalanceEventParty party) => new()
    {
        DisplayName = party.DisplayName,
        Amount = party.Amount,
    };

    /// <summary>
    /// Converts the provided Domain Account balance to an API model.
    /// </summary>
    private static AccountBalanceModel ToModel(AccountBalance balance) => new()
    {
        PostedBalance = balance.PostedBalance,
        BalanceIncludingPending = balance.BalanceIncludingPending,
    };
}