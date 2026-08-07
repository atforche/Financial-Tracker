using Domain;
using Domain.BalanceEvents;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.FundGoals;
using Models.Funds;

namespace Rest.FundGoals;

/// <summary>
/// Converts Fund Goal balance-event API models and Domain query results.
/// </summary>
public sealed class FundGoalBalanceEventConverter
{
    /// <summary>
    /// Converts an API date-range query to a Domain query.
    /// </summary>
    public FundGoalBalanceEventQuery ToDomain(FundGoalBalanceEventsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Accounting Period range query to a Domain query.
    /// </summary>
    public FundGoalBalanceEventAccountingPeriodRangeQuery ToDomain(FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Fund Goal filter to a Domain event filter.
    /// </summary>
    private static FundGoalBalanceEventFilter ToDomain(FundGoalFilterModel? filter) => new(filter?.FundIds ?? []);

    /// <summary>
    /// Converts an API sort to a Domain sort.
    /// </summary>
    private static FundGoalBalanceEventSort ToDomain(FundGoalBalanceEventSortModel? sort) => sort switch
    {
        FundGoalBalanceEventSortModel.FundName => FundGoalBalanceEventSort.FundName,
        FundGoalBalanceEventSortModel.FundNameDescending => FundGoalBalanceEventSort.FundNameDescending,
        FundGoalBalanceEventSortModel.Date => FundGoalBalanceEventSort.Date,
        FundGoalBalanceEventSortModel.DateDescending => FundGoalBalanceEventSort.DateDescending,
        FundGoalBalanceEventSortModel.Type => FundGoalBalanceEventSort.Type,
        FundGoalBalanceEventSortModel.TypeDescending => FundGoalBalanceEventSort.TypeDescending,
        FundGoalBalanceEventSortModel.Amount => FundGoalBalanceEventSort.Amount,
        FundGoalBalanceEventSortModel.AmountDescending => FundGoalBalanceEventSort.AmountDescending,
        FundGoalBalanceEventSortModel.Counterparty => FundGoalBalanceEventSort.Counterparty,
        FundGoalBalanceEventSortModel.CounterpartyDescending => FundGoalBalanceEventSort.CounterpartyDescending,
        FundGoalBalanceEventSortModel.Source => FundGoalBalanceEventSort.Source,
        FundGoalBalanceEventSortModel.SourceDescending => FundGoalBalanceEventSort.SourceDescending,
        FundGoalBalanceEventSortModel.Destination => FundGoalBalanceEventSort.Destination,
        FundGoalBalanceEventSortModel.DestinationDescending => FundGoalBalanceEventSort.DestinationDescending,
        _ => FundGoalBalanceEventSort.DateDescending,
    };

    /// <summary>
    /// Converts a Domain page to an API collection.
    /// </summary>
    public CollectionModel<FundGoalBalanceEventModel> ToModel(QueryPage<FundGoalBalanceEvent> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts a Domain Fund Goal balance event to an API model.
    /// </summary>
    public FundGoalBalanceEventModel ToModel(FundGoalBalanceEvent balanceEvent) => new()
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
        Fund = new FundModel
        {
            Id = balanceEvent.Fund.Id.Value,
            Name = balanceEvent.Fund.Name,
            Description = balanceEvent.Fund.Description,
        },
        Source = ToModel(balanceEvent.Source),
        Destinations = balanceEvent.Destinations.Select(ToModel).ToList(),
        PreviousTotals = ToModel(balanceEvent.PreviousTotals),
        NewTotals = ToModel(balanceEvent.NewTotals),
    };

    /// <summary>
    /// Converts a Domain Fund Goal balance event party to an API model.
    /// </summary>
    private static FundGoalBalanceEventPartyModel ToModel(FundGoalBalanceEventParty party) => new()
    {
        DisplayName = party.DisplayName,
        Amount = party.Amount,
    };

    /// <summary>
    /// Converts Domain Fund Goal totals to an API model.
    /// </summary>
    private static FundGoalTotalsModel ToModel(FundGoalTotals totals) => new()
    {
        AmountAssigned = totals.AmountAssigned,
        RegularAmountAssigned = totals.RegularAmountAssigned,
        AmountAssignedIncludingPending = totals.AmountAssignedIncludingPending,
        RegularAmountAssignedIncludingPending = totals.RegularAmountAssignedIncludingPending,
        AmountSpent = totals.AmountSpent,
        AmountSpentIncludingPending = totals.AmountSpentIncludingPending,
    };
}