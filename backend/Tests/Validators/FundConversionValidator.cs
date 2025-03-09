using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Fund Conversion matches the expected state
/// </summary>
internal sealed class FundConversionValidator : EntityValidatorBase<FundConversion, FundConversionState, FundConversionComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundConversion">Fund Conversion to validate</param>
    public FundConversionValidator(FundConversion fundConversion)
        : this([fundConversion])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundConversions">Fund Conversions to validate</param>
    public FundConversionValidator(IEnumerable<FundConversion> fundConversions)
        : base(fundConversions)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(FundConversionState expectedState, FundConversion entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
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

/// <summary>
/// Comparer class that compares Fund Conversions and Fund Conversion States
/// </summary>
internal sealed class FundConversionComparer : EntityComparerBase,
    IComparer<FundConversion>,
    IComparer<FundConversionState>
{
    /// <inheritdoc/>
    public int Compare(FundConversion? first, FundConversion? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.EventDate, first.EventSequence), new Key(second.EventDate, second.EventSequence));
    }

    /// <inheritdoc/>
    public int Compare(FundConversionState? first, FundConversionState? second)
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
    /// Record class representing the key used to compare a Fund Conversion or Fund Conversion State
    /// </summary>
    /// <param name="EventDate">Event Date for this Fund Conversion</param>
    /// <param name="EventSequence">Event Sequence for this Fund Conversion</param>
    private sealed record Key(DateOnly EventDate, int EventSequence);
}