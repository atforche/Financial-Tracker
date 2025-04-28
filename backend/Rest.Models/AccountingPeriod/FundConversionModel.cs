using System.Text.Json.Serialization;
using Rest.Models.Account;
using Rest.Models.Fund;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a Fund Conversion
/// </summary>
public class FundConversionModel
{
    /// <inheritdoc cref="Domain.Aggregates.EntityBase.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.BalanceEventBase.Account"/>
    public AccountModel Account { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.BalanceEventBase.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.FromFund"/>
    public FundModel FromFund { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.ToFund"/>
    public FundModel ToFund { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.Amount"/>
    public decimal Amount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public FundConversionModel(Guid id,
        AccountModel account,
        DateOnly eventDate,
        FundModel fromFund,
        FundModel toFund,
        decimal amount)
    {
        Id = id;
        Account = account;
        EventDate = eventDate;
        FromFund = fromFund;
        ToFund = toFund;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundConversion">Fund Conversion entity to build this Fund Conversion REST model from</param>
    public FundConversionModel(Domain.Aggregates.AccountingPeriods.FundConversion fundConversion)
    {
        Id = fundConversion.Id.ExternalId;
        Account = new AccountModel(fundConversion.Account);
        EventDate = fundConversion.EventDate;
        FromFund = new FundModel(fundConversion.FromFund);
        ToFund = new FundModel(fundConversion.ToFund);
        Amount = fundConversion.Amount;
    }
}