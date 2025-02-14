using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Transaction Balance Event matches the expected state
/// </summary>
internal sealed class TransactionBalanceEventValidator : EntityValidatorBase<TransactionBalanceEvent,
    TransactionBalanceEventState,
    TransactionBalanceEventComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transactionBalanceEvent">Transaction Balance Event to validate</param>
    public TransactionBalanceEventValidator(TransactionBalanceEvent transactionBalanceEvent)
        : this([transactionBalanceEvent])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transactionBalanceEvents">Transaction Balance Events to validate</param>
    public TransactionBalanceEventValidator(IEnumerable<TransactionBalanceEvent> transactionBalanceEvents)
        : base(transactionBalanceEvents)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(TransactionBalanceEventState expectedState, TransactionBalanceEvent entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        Assert.Equal(expectedState.TransactionEventType, entity.TransactionEventType);
        Assert.Equal(expectedState.TransactionAccountType, entity.TransactionAccountType);
    }
}

/// <summary>
/// Record class representing the state of a Transaction Balance Event
/// </summary>
internal sealed record TransactionBalanceEventState
{
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

/// <summary>
/// Comparer class that compares Transaction Balance Events and Transaction Balance Event States
/// </summary>
internal sealed class TransactionBalanceEventComparer : EntityComparerBase,
    IComparer<TransactionBalanceEvent>,
    IComparer<TransactionBalanceEventState>
{
    /// <inheritdoc/>
    public int Compare(TransactionBalanceEvent? first, TransactionBalanceEvent? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.EventDate, first.EventSequence), new Key(second.EventDate, second.EventSequence));
    }

    /// <inheritdoc/>
    public int Compare(TransactionBalanceEventState? first, TransactionBalanceEventState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.EventDate, first.EventSequence), new Key(second.EventDate, second.EventSequence));
    }

    /// <summary>
    /// Compares the provided keys to determine their ordering
    /// </summary>
    /// <param name="first">First Key to compare</param>
    /// <param name="second">Second Key to compare</param>
    /// <returns>The ordering of the provided keys</returns>
    private static int ComparePrivate(Key first, Key second)
    {
        if (first.EventDate.CompareTo(second.EventDate) != 0)
        {
            return first.EventDate.CompareTo(second.EventDate);
        }
        return first.EventSequence.CompareTo(second.EventSequence);
    }

    /// <summary>
    /// Record class representing the key used to compare a Transaction Balance Event or Transaction Balance Event State
    /// </summary>
    /// <param name="EventDate">Event Date for this Transaction Balance Event</param>
    /// <param name="EventSequence">Event Sequence for this Transaction Balance Event</param>
    private sealed record Key(DateOnly EventDate, int EventSequence);
}