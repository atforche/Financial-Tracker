using Domain;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Funds.Queries;
using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converts Fund balance-event API models and Domain query results.
/// </summary>
public sealed class FundBalanceEventConverter
{
    /// <summary>
    /// Converts an API date-range query to a Domain query.
    /// </summary>
    public FundBalanceEventQuery ToDomain(FundBalanceEventsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        new FundFilter(model.Filter?.NameSearch, model.Filter?.Names ?? []),
        model.Sort switch
        {
            FundBalanceEventSortModel.FundName => FundBalanceEventSort.FundName,
            FundBalanceEventSortModel.FundNameDescending => FundBalanceEventSort.FundNameDescending,
            FundBalanceEventSortModel.AccountingPeriodName => FundBalanceEventSort.AccountingPeriod,
            FundBalanceEventSortModel.AccountingPeriodNameDescending => FundBalanceEventSort.AccountingPeriodDescending,
            FundBalanceEventSortModel.Date => FundBalanceEventSort.Date,
            FundBalanceEventSortModel.DateDescending => FundBalanceEventSort.DateDescending,
            FundBalanceEventSortModel.Type => FundBalanceEventSort.Type,
            FundBalanceEventSortModel.TypeDescending => FundBalanceEventSort.TypeDescending,
            FundBalanceEventSortModel.Amount => FundBalanceEventSort.Amount,
            FundBalanceEventSortModel.AmountDescending => FundBalanceEventSort.AmountDescending,
            _ => FundBalanceEventSort.DateDescending,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts a Domain page to an API collection.
    /// </summary>
    public CollectionModel<FundBalanceEventModel> ToModel(QueryPage<FundBalanceEvent> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts a Domain Fund balance event to an API model.
    /// </summary>
    private static FundBalanceEventModel ToModel(FundBalanceEvent balanceEvent) => new()
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
        IsPosted = true,
        Amount = balanceEvent.Amount,
        Fund = new FundModel
        {
            Id = balanceEvent.Fund.Id.Value,
            Name = balanceEvent.Fund.Name,
            Description = balanceEvent.Fund.Description,
        },
        PreviousBalance = ToModel(balanceEvent.PreviousBalance),
        NewBalance = ToModel(balanceEvent.NewBalance),
    };

    /// <summary>
    /// Converts a Domain Fund balance to an API model.
    /// </summary>
    private static FundBalanceModel ToModel(FundBalance balance) => new()
    {
        PostedBalance = balance.PostedBalance,
        PendingDebitAmount = balance.PendingDebitAmount,
        PendingCreditAmount = balance.PendingCreditAmount,
    };
}