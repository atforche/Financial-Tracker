using System.Text.Json.Serialization;
using Domain.AccountingPeriods;
using Rest.Models.Accounts;
using Rest.Models.Funds;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a <see cref="ChangeInValue"/>
/// </summary>
public class ChangeInValueModel
{
    /// <inheritdoc cref="Domain.EntityOld.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.Account"/>
    public AccountModel Account { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.AccountingEntry"/>
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
    public ChangeInValueModel(ChangeInValue changeInValue)
    {
        Id = changeInValue.Id.Value;
        Account = new AccountModel(changeInValue.Account);
        EventDate = changeInValue.EventDate;
        AccountingEntry = new FundAmountModel(changeInValue.AccountingEntry);
    }
}