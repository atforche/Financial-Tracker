using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="ChangeInValue"/> matches the expected state
/// </summary>
internal sealed class ChangeInValueValidator : EntityValidator<ChangeInValue, ChangeInValueState>
{
    /// <inheritdoc/>
    public override void Validate(ChangeInValue entity, ChangeInValueState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.AccountingPeriodId, entity.AccountingPeriodId);
        Assert.Equal(expectedState.AccountId, entity.AccountId);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        new FundAmountValidator().Validate(entity.FundAmount, expectedState.FundAmount);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="ChangeInValue"/>
/// </summary>
internal sealed record ChangeInValueState
{
    /// <summary>
    /// Accounting Period ID for this Change In Value
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Account ID for this Change In Value
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Event Date for this Change In Value
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Change In Value
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Fund Amount for this Change In Value
    /// </summary>
    public required FundAmountState FundAmount { get; init; }
}