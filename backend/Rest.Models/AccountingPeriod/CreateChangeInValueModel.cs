using Rest.Models.FundAmount;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Change In Value
/// </summary>
public class CreateChangeInValueModel
{
    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.ChangeInValue.AccountingEntry"/>
    public required CreateFundAmountModel AccountingEntry { get; init; }
}