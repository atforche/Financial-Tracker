using Domain.FundConversions;

namespace Rest.Models.FundConversions;

/// <summary>
/// REST model representing a request to create a <see cref="FundConversion"/>
/// </summary>
public class CreateFundConversionModel
{
    /// <inheritdoc cref="FundConversion.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="FundConversion.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="FundConversion.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="FundConversion.FromFundId"/>
    public required Guid FromFundId { get; init; }

    /// <inheritdoc cref="FundConversion.ToFundId"/>
    public required Guid ToFundId { get; init; }

    /// <inheritdoc cref="FundConversion.Amount"/>
    public required decimal Amount { get; init; }
}