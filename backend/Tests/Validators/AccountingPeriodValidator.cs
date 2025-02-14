using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Accounting Period matches the expected state
/// </summary>
internal sealed class AccountingPeriodValidator : EntityValidatorBase<AccountingPeriod, AccountingPeriodState, AccountingPeriodComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to validate</param>
    public AccountingPeriodValidator(AccountingPeriod accountingPeriod)
        : this([accountingPeriod])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriods">Accounting Periods to validate</param>
    public AccountingPeriodValidator(IEnumerable<AccountingPeriod> accountingPeriods)
        : base(accountingPeriods)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(AccountingPeriodState expectedState, AccountingPeriod entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.Year, entity.Year);
        Assert.Equal(expectedState.Month, entity.Month);
        Assert.Equal(expectedState.IsOpen, entity.IsOpen);
        new AccountBalanceCheckpointValidator(entity.AccountBalanceCheckpoints).Validate(expectedState.AccountBalanceCheckpoints);
        new TransactionValidator(entity.Transactions).Validate(expectedState.Transactions);
    }
}

/// <summary>
/// Record class representing the state of an Accounting Period
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

    /// <summary>
    /// Account Balance Checkpoints for this Accounting Period
    /// </summary>
    public required List<AccountBalanceCheckpointState> AccountBalanceCheckpoints { get; init; }

    /// <summary>
    /// Transactions for this Accounting Period
    /// </summary>
    public required List<TransactionState> Transactions { get; init; }
}

/// <summary>
/// Comparer class that compares Accounting Periods and Accounting Period States
/// </summary>
internal sealed class AccountingPeriodComparer : EntityComparerBase, IComparer<AccountingPeriod>, IComparer<AccountingPeriodState>
{
    /// <inheritdoc/>
    public int Compare(AccountingPeriod? first, AccountingPeriod? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.Year, first.Month), new Key(second.Year, second.Month));
    }

    /// <inheritdoc/>
    public int Compare(AccountingPeriodState? first, AccountingPeriodState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.Year, first.Month), new Key(second.Year, second.Month));
    }

    /// <summary>
    /// Compares the provided keys to determine their ordering
    /// </summary>
    /// <param name="first">First Key to compare</param>
    /// <param name="second">Second Key to compare</param>
    /// <returns>The ordering of the provided keys</returns>
    private static int ComparePrivate(Key first, Key second)
    {
        if (first.Year.CompareTo(second.Year) != 0)
        {
            return first.Year.CompareTo(second.Year);
        }
        return first.Month.CompareTo(second.Month);
    }

    /// <summary>
    /// Record class representing the key used to compare an Accounting Period or Accounting Period State
    /// </summary>
    /// <param name="Year">Year for this Accounting Period</param>
    /// <param name="Month">Month for this Accounting Period</param>
    private sealed record Key(int Year, int Month);
}