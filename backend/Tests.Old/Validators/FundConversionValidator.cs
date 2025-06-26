using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundConversions;
using Domain.Funds;

namespace Tests.Old.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="FundConversion"/> matches the expected state
/// </summary>
internal sealed class FundConversionValidator : EntityValidator<FundConversion, FundConversionState>
{
    /// <inheritdoc/>
    public override void Validate(FundConversion entity, FundConversionState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.AccountingPeriodId, entity.AccountingPeriodId);
        Assert.Equal(expectedState.AccountId, entity.AccountId);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        Assert.Equal(expectedState.FromFundId, entity.FromFundId);
        Assert.Equal(expectedState.ToFundId, entity.ToFundId);
        Assert.Equal(expectedState.Amount, entity.Amount);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="FundConversion"/>
/// </summary>
internal sealed record FundConversionState
{
    /// <summary>
    /// Accounting Period ID for this Fund Conversion
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Event Date for this Fund Conversion
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Fund Conversion
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Account ID for this Fund Conversion
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// From Fund ID for this Fund Conversion
    /// </summary>
    public required FundId FromFundId { get; init; }

    /// <summary>
    /// To Fund ID for this Fund Conversion
    /// </summary>
    public required FundId ToFundId { get; init; }

    /// <summary>
    /// Amount for this Fund Conversion
    /// </summary>
    public required decimal Amount { get; init; }
}