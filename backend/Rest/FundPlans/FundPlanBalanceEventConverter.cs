using Domain;
using Domain.BalanceEvents;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.FundPlans;
using Models.Funds;

namespace Rest.FundPlans;

/// <summary>
/// Converts Fund Plan balance-event API models and Domain query results.
/// </summary>
public sealed class FundPlanBalanceEventConverter
{
    /// <summary>
    /// Converts an API date-range query to a Domain query.
    /// </summary>
    public FundPlanBalanceEventQuery ToDomain(FundPlanBalanceEventsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Accounting Period range query to a Domain query.
    /// </summary>
    public FundPlanBalanceEventAccountingPeriodRangeQuery ToDomain(FundPlanBalanceEventsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Fund Plan filter to a Domain event filter.
    /// </summary>
    private static FundPlanBalanceEventFilter ToDomain(FundPlanFilterModel? filter) => new(filter?.FundIds ?? []);

    /// <summary>
    /// Converts an API sort to a Domain sort.
    /// </summary>
    private static FundPlanBalanceEventSort ToDomain(FundPlanBalanceEventSortModel? sort) => sort switch
    {
        FundPlanBalanceEventSortModel.FundName => FundPlanBalanceEventSort.FundName,
        FundPlanBalanceEventSortModel.FundNameDescending => FundPlanBalanceEventSort.FundNameDescending,
        FundPlanBalanceEventSortModel.Date => FundPlanBalanceEventSort.Date,
        FundPlanBalanceEventSortModel.DateDescending => FundPlanBalanceEventSort.DateDescending,
        FundPlanBalanceEventSortModel.Type => FundPlanBalanceEventSort.Type,
        FundPlanBalanceEventSortModel.TypeDescending => FundPlanBalanceEventSort.TypeDescending,
        FundPlanBalanceEventSortModel.Amount => FundPlanBalanceEventSort.Amount,
        FundPlanBalanceEventSortModel.AmountDescending => FundPlanBalanceEventSort.AmountDescending,
        _ => FundPlanBalanceEventSort.DateDescending,
    };

    /// <summary>
    /// Converts a Domain page to an API collection.
    /// </summary>
    public CollectionModel<FundPlanBalanceEventModel> ToModel(QueryPage<FundPlanBalanceEvent> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts a Domain Fund Plan balance event to an API model.
    /// </summary>
    public FundPlanBalanceEventModel ToModel(FundPlanBalanceEvent balanceEvent) => new()
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
        PreviousTotals = ToModel(balanceEvent.PreviousTotals),
        NewTotals = ToModel(balanceEvent.NewTotals),
    };

    /// <summary>
    /// Converts Domain Fund Plan totals to an API model.
    /// </summary>
    private static FundPlanTotalsModel ToModel(FundPlanTotals totals) => new()
    {
        AmountAssigned = totals.AmountAssigned,
        PendingAmountAssigned = totals.PendingAmountAssigned,
        AmountSpent = totals.AmountSpent,
        PendingAmountSpent = totals.PendingAmountSpent,
    };
}