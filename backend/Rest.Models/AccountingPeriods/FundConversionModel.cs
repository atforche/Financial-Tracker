using System.Text.Json.Serialization;
using Domain.AccountingPeriods;
using Domain.BalanceEvents;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a <see cref="FundConversion"/>
/// </summary>
public class FundConversionModel
{
    /// <inheritdoc cref="Domain.EntityOld.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="IBalanceEvent.AccountId"/>
    public Guid AccountId { get; init; }

    /// <inheritdoc cref="IBalanceEvent.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="FundConversion.FromFundId"/>
    public Guid FromFundId { get; init; }

    /// <inheritdoc cref="FundConversion.ToFundId"/>
    public Guid ToFundId { get; init; }

    /// <inheritdoc cref="FundConversion.Amount"/>
    public decimal Amount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public FundConversionModel(Guid id,
        Guid accountId,
        DateOnly eventDate,
        Guid fromFundId,
        Guid toFundId,
        decimal amount)
    {
        Id = id;
        AccountId = accountId;
        EventDate = eventDate;
        FromFundId = fromFundId;
        ToFundId = toFundId;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundConversion">Fund Conversion entity to build this Fund Conversion REST model from</param>
    public FundConversionModel(FundConversion fundConversion)
    {
        Id = fundConversion.Id.Value;
        AccountId = fundConversion.AccountId.Value;
        EventDate = fundConversion.EventDate;
        FromFundId = fundConversion.FromFundId.Value;
        ToFundId = fundConversion.ToFundId.Value;
        Amount = fundConversion.Amount;
    }
}