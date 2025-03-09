using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Fund Amounts match the expected states
/// </summary>
internal sealed class FundAmountValidator(IEnumerable<FundAmount> fundAmounts) :
    EntityValidatorBase<FundAmount, FundAmountState, FundAmountComparer>(fundAmounts)
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundAmount">Fund Amount to validate</param>
    public FundAmountValidator(FundAmount fundAmount)
        : this([fundAmount])
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(FundAmountState expectedState, FundAmount entity)
    {
        Assert.Equal(expectedState.FundName, entity.Fund.Name);
        Assert.Equal(expectedState.Amount, entity.Amount);
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

/// <summary>
/// Comparer class that compares Fund Amounts and Fund Amount States
/// </summary>
internal sealed class FundAmountComparer : EntityComparerBase, IComparer<FundAmount>, IComparer<FundAmountState>
{
    /// <inheritdoc/>
    public int Compare(FundAmount? first, FundAmount? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.Fund.Name, first.Amount), new Key(second.Fund.Name, second.Amount));
    }

    /// <inheritdoc/>
    public int Compare(FundAmountState? first, FundAmountState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.FundName, first.Amount), new Key(second.FundName, second.Amount));
    }

    /// <summary>
    /// Compares the provided keys to determine their ordering
    /// </summary>
    /// <param name="first">First Key to compare</param>
    /// <param name="second">Second Key to compare</param>
    /// <returns>The ordering of the provided keys</returns>
    private static int ComparePrivate(Key first, Key second)
    {
        if (!string.Equals(first.FundName, second.FundName, StringComparison.OrdinalIgnoreCase))
        {
            return string.Compare(first.FundName, second.FundName, StringComparison.OrdinalIgnoreCase);
        }
        return first.Amount.CompareTo(second.Amount);
    }

    /// <summary>
    /// Record class representing the key used to compare a Fund Amount or Fund Amount State
    /// </summary>
    /// <param name="FundName">Fund Name for this Fund Amount</param>
    /// <param name="Amount">Amount for this Fund Amount</param>
    private sealed record Key(string FundName, decimal Amount);
}