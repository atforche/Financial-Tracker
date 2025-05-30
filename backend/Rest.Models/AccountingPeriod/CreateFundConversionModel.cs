namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Fund Conversion
/// </summary>
public class CreateFundConversionModel
{
    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.FundConversion.FromFund"/>
    public required Guid FromFundId { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.FundConversion.ToFund"/>
    public required Guid ToFundId { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.FundConversion.Amount"/>
    public required decimal Amount { get; init; }
}