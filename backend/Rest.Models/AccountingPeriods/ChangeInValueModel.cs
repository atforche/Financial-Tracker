using System.Text.Json.Serialization;
using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Rest.Models.Funds;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a <see cref="ChangeInValue"/>
/// </summary>
public class ChangeInValueModel
{
    /// <inheritdoc cref="Domain.EntityOld.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="IBalanceEvent.AccountId"/>
    public Guid AccountId { get; init; }

    /// <inheritdoc cref="IBalanceEvent.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.FundAmount"/>
    public FundAmountModel FundAmount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public ChangeInValueModel(Guid id,
        Guid accountId,
        DateOnly eventDate,
        FundAmountModel fundAmount)
    {
        Id = id;
        AccountId = accountId;
        EventDate = eventDate;
        FundAmount = fundAmount;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="changeInValue">Change In Value entity to builder this Change In Value REST model from</param>
    public ChangeInValueModel(ChangeInValue changeInValue)
    {
        Id = changeInValue.Id.Value;
        AccountId = changeInValue.AccountId.Value;
        EventDate = changeInValue.EventDate;
        FundAmount = new FundAmountModel(changeInValue.FundAmount);
    }
}