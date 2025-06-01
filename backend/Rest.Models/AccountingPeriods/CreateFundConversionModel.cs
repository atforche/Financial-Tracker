using Domain.AccountingPeriods;
using Domain.BalanceEvents;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a request to create a <see cref="FundConversion"/>
/// </summary>
public class CreateFundConversionModel
{
    /// <inheritdoc cref="IBalanceEvent.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="IBalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="FundConversion.FromFundId"/>
    public required Guid FromFundId { get; init; }

    /// <inheritdoc cref="FundConversion.ToFundId"/>
    public required Guid ToFundId { get; init; }

    /// <inheritdoc cref="FundConversion.Amount"/>
    public required decimal Amount { get; init; }
}