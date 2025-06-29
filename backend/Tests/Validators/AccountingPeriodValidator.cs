using Domain.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountingPeriod"/> matches the expected state
/// </summary>
internal sealed class AccountingPeriodValidator : Validator<AccountingPeriod, AccountingPeriodState>
{
    /// <inheritdoc/>
    public override void Validate(AccountingPeriod entity, AccountingPeriodState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.Year, entity.Year);
        Assert.Equal(expectedState.Month, entity.Month);
        Assert.Equal(expectedState.IsOpen, entity.IsOpen);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountingPeriod"/>
/// </summary>
internal sealed record AccountingPeriodState
{
    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// Is Open Flag for this Accounting Period
    /// </summary>
    public required bool IsOpen { get; init; }
}