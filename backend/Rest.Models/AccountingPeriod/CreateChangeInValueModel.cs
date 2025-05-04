using Rest.Models.FundAmount;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Change In Value
/// </summary>
public class CreateChangeInValueModel
{
    /// <inheritdoc cref="Domain.Aggregates.BalanceEvent.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.BalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.ChangeInValue.AccountingEntry"/>
    public required CreateFundAmountModel AccountingEntry { get; init; }
}