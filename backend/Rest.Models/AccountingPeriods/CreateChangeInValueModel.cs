using Domain.AccountingPeriods;
using Rest.Models.Funds;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a request to create a <see cref="ChangeInValue"/>
/// </summary>
public class CreateChangeInValueModel
{
    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.AccountingEntry"/>
    public required CreateFundAmountModel AccountingEntry { get; init; }
}