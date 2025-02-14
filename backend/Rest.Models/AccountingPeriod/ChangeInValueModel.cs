using System.Text.Json.Serialization;
using Rest.Models.Account;
using Rest.Models.FundAmount;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a Change In Value
/// </summary>
public class ChangeInValueModel
{
    /// <inheritdoc cref="Domain.Aggregates.EntityBase.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.BalanceEventBase.Account"/>
    public AccountModel Account { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.BalanceEventBase.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.ChangeInValue.AccountingEntry"/>
    public FundAmountModel AccountingEntry { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public ChangeInValueModel(Guid id,
        AccountModel account,
        DateOnly eventDate,
        FundAmountModel accountingEntry)
    {
        Id = id;
        Account = account;
        EventDate = eventDate;
        AccountingEntry = accountingEntry;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="changeInValue">Change In Value entity to builder this Change In Value REST model from</param>
    public ChangeInValueModel(Domain.Aggregates.AccountingPeriods.ChangeInValue changeInValue)
    {
        Id = changeInValue.Id.ExternalId;
        Account = new AccountModel(changeInValue.Account);
        EventDate = changeInValue.EventDate;
        AccountingEntry = new FundAmountModel(changeInValue.AccountingEntry);
    }
}