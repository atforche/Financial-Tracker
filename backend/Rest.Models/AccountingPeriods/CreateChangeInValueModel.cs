using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Rest.Models.Funds;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a request to create a <see cref="ChangeInValue"/>
/// </summary>
public class CreateChangeInValueModel
{
    /// <inheritdoc cref="IBalanceEvent.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="IBalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.FundAmount"/>
    public required CreateFundAmountModel FundAmount { get; init; }
}