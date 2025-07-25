using Domain.Funds;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="FundAmount"/> match the expected states
/// </summary>
internal sealed class FundAmountValidator : Validator<FundAmount, FundAmountState>
{
    /// <inheritdoc/>
    public override void Validate(FundAmount entityToValidate, FundAmountState expectedState)
    {
        Assert.Equal(expectedState.FundId, entityToValidate.FundId);
        Assert.Equal(expectedState.Amount, entityToValidate.Amount);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="FundAmount"/>
/// </summary>
internal sealed record FundAmountState
{
    /// <summary>
    /// Fund ID for this Fund Amount
    /// </summary>
    public required FundId FundId { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}