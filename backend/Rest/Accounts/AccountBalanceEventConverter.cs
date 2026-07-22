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
    /// Converts the provided API query to a Domain query.
    /// </summary>
    public AccountBalanceEventQuery ToDomain(AccountBalanceEventsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        new AccountFilter(
            model.Filter?.NameSearch,
            model.Filter?.Names ?? [],
            model.Filter?.Types?.Select(type => (AccountType)type).ToList() ?? []),
        model.Sort switch
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
            _ => AccountBalanceEventSort.DateDescending,
        },
        model.Offset ?? 0,
        model.Limit);

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
    private static AccountBalanceEventModel ToModel(AccountBalanceEvent balanceEvent) => new()
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
        Date = balanceEvent.Date,
        Type = balanceEvent.Type == BalanceEventType.Debit ? BalanceEventTypeModel.Debit : BalanceEventTypeModel.Credit,
        IsPosted = balanceEvent.IsPosted,
        Amount = balanceEvent.Amount,
        Account = new AccountModel
        {
            Id = balanceEvent.Account.Id.Value,
            Name = balanceEvent.Account.Name,
            Type = AccountTypeConverter.ToModel(balanceEvent.Account.Type),
        },
        PreviousBalance = ToModel(balanceEvent.PreviousBalance),
        NewBalance = ToModel(balanceEvent.NewBalance),
    };

    /// <summary>
    /// Converts the provided Domain Account balance to an API model.
    /// </summary>
    private static AccountBalanceModel ToModel(AccountBalance balance) => new()
    {
        PostedBalance = balance.PostedBalance,
        PendingDebitAmount = balance.PendingDebitAmount,
        PendingCreditAmount = balance.PendingCreditAmount,
        PendingBalance = balance.PendingBalance,
    };
}