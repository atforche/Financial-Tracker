using System.Text.Json.Serialization;
using Domain.ChangeInValues;
using Rest.Models.Funds;

namespace Rest.Models.ChangeInValues;

/// <summary>
/// REST model representing a <see cref="ChangeInValue"/>
/// </summary>
public class ChangeInValueModel
{
    /// <inheritdoc cref="ChangeInValueId"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="ChangeInValue.AccountingPeriodId"/>
    public Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="ChangeInValue.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.AccountId"/>
    public Guid AccountId { get; init; }

    /// <inheritdoc cref="ChangeInValue.FundAmount"/>
    public FundAmountModel FundAmount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public ChangeInValueModel(Guid id,
        Guid accountingPeriodId,
        Guid accountId,
        DateOnly eventDate,
        FundAmountModel fundAmount)
    {
        Id = id;
        AccountingPeriodId = accountingPeriodId;
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
        AccountingPeriodId = changeInValue.AccountingPeriodId.Value;
        AccountId = changeInValue.AccountId.Value;
        EventDate = changeInValue.EventDate;
        FundAmount = new FundAmountModel(changeInValue.FundAmount);
    }
}