using Domain.AccountingPeriods;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing a request to create a <see cref="FundConversion"/>
/// </summary>
public class CreateFundConversionModel
{
    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="FundConversion.FromFund"/>
    public required Guid FromFundId { get; init; }

    /// <inheritdoc cref="FundConversion.ToFund"/>
    public required Guid ToFundId { get; init; }

    /// <inheritdoc cref="FundConversion.Amount"/>
    public required decimal Amount { get; init; }
}