using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="TransactionBalanceEvent"/> matches the expected state
/// </summary>
internal sealed class TransactionBalanceEventValidator : EntityValidator<TransactionBalanceEvent, TransactionBalanceEventState>
{
    /// <inheritdoc/>
    public override void Validate(TransactionBalanceEvent entity, TransactionBalanceEventState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.AccountingPeriodId, entity.AccountingPeriodId);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);

        Assert.Equal(expectedState.Parts.Count, entity.Parts.Count);
        foreach ((TransactionBalanceEventPartType expectedPart, TransactionBalanceEventPart part) in expectedState.Parts.Zip(entity.Parts))
        {
            Assert.Equal(expectedPart, part.Type);
        }
    }
}

/// <summary>
/// Record class representing the state of a <see cref="TransactionBalanceEvent"/>
/// </summary>
internal sealed record TransactionBalanceEventState
{
    /// <summary>
    /// Accounting Period ID for this Transaction Balance Event
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Event Date for this Transaction Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Transaction Balance Event
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Transaction Balance Event Parts for this Transaction Balance Event
    /// </summary>
    public required IReadOnlyCollection<TransactionBalanceEventPartType> Parts { get; init; }
}