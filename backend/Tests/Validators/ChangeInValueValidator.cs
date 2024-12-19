using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Change In Value matches the expected state
/// </summary>
internal sealed class ChangeInValueValidator : EntityValidatorBase<ChangeInValue,
    ChangeInValueState,
    ChangeInValueComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="changeInValue">Change In Value to validate</param>
    public ChangeInValueValidator(ChangeInValue changeInValue)
        : this([changeInValue])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="changeInValues">Change In Values to validate</param>
    public ChangeInValueValidator(IEnumerable<ChangeInValue> changeInValues)
        : base(changeInValues)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(ChangeInValueState expectedState, ChangeInValue entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.EventDate, entity.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.EventSequence);
        new FundAmountValidator(entity.AccountingEntry).Validate(expectedState.AccountingEntry);
    }
}

/// <summary>
/// Record class representing the state of a Change In Value
/// </summary>
internal sealed record ChangeInValueState
{
    /// <summary>
    /// Account Name for this Change In Value
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Change In Value
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Change In Value
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Accounting Entry for this Change In Value
    /// </summary>
    public required FundAmountState AccountingEntry { get; init; }
}

/// <summary>
/// Comparer class that compares Fund Conversions and Fund Conversion States
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal sealed class ChangeInValueComparer : EntityComparerBase,
    IComparer<ChangeInValue>,
    IComparer<ChangeInValueState>
{
    /// <inheritdoc/>
    public int Compare(ChangeInValue? first, ChangeInValue? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.EventDate, first.EventSequence), new Key(second.EventDate, second.EventSequence));
    }

    /// <inheritdoc/>
    public int Compare(ChangeInValueState? first, ChangeInValueState? second)
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
    /// Record class representing the key used to compare a Fund Conversion or Fund Conversino State
    /// </summary>
    /// <param name="EventDate">Event Date for this Fund Conversion</param>
    /// <param name="EventSequence">Event Sequence for this Fund Conversion</param>
    private sealed record Key(DateOnly EventDate, int EventSequence);
}