using Domain.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="TransactionBalanceEvent"/> matches the expected state
/// </summary>
internal sealed class TransactionBalanceEventValidator : EntityValidator<TransactionBalanceEvent, TransactionBalanceEventState>
{
    /// <inheritdoc/>
    public override void Validate(TransactionBalanceEvent entity, TransactionBalanceEventState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountingPeriodKey, entity.AccountingPeriodKey);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        Assert.Equal(expectedState.TransactionEventType, entity.TransactionEventType);
        Assert.Equal(expectedState.TransactionAccountType, entity.TransactionAccountType);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="TransactionBalanceEvent"/>
/// </summary>
internal sealed record TransactionBalanceEventState
{
    /// <summary>
    /// Accounting Period Key for this Transaction Balance Event
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Account Name for this Transaction Balance Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Transaction Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Transaction Balance Event
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Transaction Event Type for this Transaction Balance Event
    /// </summary>
    public required TransactionBalanceEventType TransactionEventType { get; init; }

    /// <summary>
    /// Transaction Account Type for this Transaction Balance Event
    /// </summary>
    public required TransactionAccountType TransactionAccountType { get; init; }
}