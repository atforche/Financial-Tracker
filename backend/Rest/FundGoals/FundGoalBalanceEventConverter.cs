using Domain.BalanceEvents;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
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
    /// Converts an API Accounting Period range query to a Domain query.
    /// </summary>
    public FundGoalBalanceEventAccountingPeriodRangeQuery ToDomain(FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter));

    /// <summary>
    /// Converts an API Fund Goal filter to a Domain event filter.
    /// </summary>
    private static FundGoalBalanceEventFilter ToDomain(FundGoalFilterModel? filter) => new(filter?.FundIds ?? []);

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
        Description = balanceEvent.Description,
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
        PreviousTotals = balanceEvent.PreviousTotals == null ? null : ToModel(balanceEvent.PreviousTotals),
        NewTotals = balanceEvent.NewTotals == null ? null : ToModel(balanceEvent.NewTotals),
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
        AmountAssignedToExpectedContribution = totals.AmountAssignedToExpectedContribution,
        RemainingAmountToAssignToExpectedContribution = totals.RemainingAmountToAssignToExpectedContribution,
        AmountSpent = totals.AmountSpent,
    };
}
