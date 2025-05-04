using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Fund Conversion matches the expected state
/// </summary>
internal sealed class FundConversionValidator : EntityValidator<FundConversion, FundConversionState>
{
    /// <inheritdoc/>
    public override void Validate(FundConversion entity, FundConversionState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountingPeriodKey, entity.AccountingPeriodKey);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        Assert.Equal(expectedState.FromFundName, entity.FromFund.Name);
        Assert.Equal(expectedState.ToFundName, entity.ToFund.Name);
        Assert.Equal(expectedState.Amount, entity.Amount);
    }
}

/// <summary>
/// Record class representing the state of a Fund Conversion
/// </summary>
internal sealed record FundConversionState
{
    /// <summary>
    /// Accounting Period Key for this Fund Conversion
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Account Name for this Fund Conversion
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Fund Conversion
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Fund Conversion
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// From Fund for this Fund Conversion
    /// </summary>
    public required string FromFundName { get; init; }

    /// <summary>
    /// To Fund for this Fund Conversion
    /// </summary>
    public required string ToFundName { get; init; }

    /// <summary>
    /// Amount for this Fund Conversion
    /// </summary>
    public required decimal Amount { get; init; }
}