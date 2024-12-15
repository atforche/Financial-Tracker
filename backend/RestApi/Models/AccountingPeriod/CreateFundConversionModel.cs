namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to create a Fund Conversion
/// </summary>
public class CreateFundConversionModel
{
    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.BalanceEventBase.Account"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.BalanceEventBase.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.FromFund"/>
    public required Guid FromFundId { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.ToFund"/>
    public required Guid ToFundId { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.AccountingPeriods.FundConversion.Amount"/>
    public required decimal Amount { get; init; }
}