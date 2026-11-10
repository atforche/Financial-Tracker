using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Funds.Queries;
using Models.BalanceEvents;
using Models.Funds;
using Rest.AccountingPeriods;

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
        ToDomain(model.Filter),
        model.IncludePending,
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Fund filter to a Domain filter.
    /// </summary>
    private static FundFilter ToDomain(FundFilterModel? filter) => new(filter?.NameSearch, filter?.Names ?? [], filter?.Ids ?? []);

    /// <summary>
    /// Converts a Domain Fund balance event to an API model.
    /// </summary>
    public FundBalanceEventModel ToModel(FundBalanceEvent balanceEvent) => new()
    {
        AccountingPeriod = AccountingPeriodConverter.ToModel(balanceEvent.AccountingPeriod),
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
        PreviousBalance = balanceEvent.PreviousBalance == null ? null : ToModel(balanceEvent.PreviousBalance),
        NewBalance = balanceEvent.NewBalance == null ? null : ToModel(balanceEvent.NewBalance),
    };

    /// <summary>
    /// Converts a Domain Fund balance event party to an API model.
    /// </summary>
    private static FundBalanceEventPartyModel ToModel(FundBalanceEventParty party) => new()
    {
        DisplayName = party.DisplayName,
        Amount = party.Amount,
    };

    /// <summary>
    /// Converts a Domain Fund balance to an API model.
    /// </summary>
    private static FundBalanceModel ToModel(FundBalance balance) => new()
    {
        PostedBalance = balance.PostedBalance,
    };
}
