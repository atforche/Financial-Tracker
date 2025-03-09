using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Fund Amounts match the expected states
/// </summary>
internal sealed class FundAmountValidator : EntityValidatorBase<FundAmount, FundAmountState>
{
    /// <inheritdoc/>
    public override void Validate(FundAmount entityToValidate, FundAmountState expectedState)
    {
        Assert.Equal(expectedState.FundName, entityToValidate.Fund.Name);
        Assert.Equal(expectedState.Amount, entityToValidate.Amount);
    }
}

/// <summary>
/// Record class representing the state of a Fund Amount
/// </summary>
internal sealed record FundAmountState
{
    /// <summary>
    /// Fund Name for this Fund Amount
    /// </summary>
    public required string FundName { get; init; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public required decimal Amount { get; init; }
}