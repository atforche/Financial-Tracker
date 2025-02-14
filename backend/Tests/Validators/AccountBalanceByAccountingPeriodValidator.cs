using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Account Balance by Accounting Period matches the expected state
/// </summary>
internal sealed class AccountBalanceByAccountingPeriodValidator : EntityValidatorBase<AccountBalanceByAccountingPeriod,
    AccountBalanceByAccountingPeriodState, AccountBalanceByAccountingPeriodComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByAccountingPeriod">Account Balance by Accounting Period to validate</param>
    public AccountBalanceByAccountingPeriodValidator(AccountBalanceByAccountingPeriod accountBalanceByAccountingPeriod)
        : this([accountBalanceByAccountingPeriod])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByAccountingPeriods">Account Balance by Accounting Periods to validate</param>
    public AccountBalanceByAccountingPeriodValidator(IEnumerable<AccountBalanceByAccountingPeriod> accountBalanceByAccountingPeriods)
        : base(accountBalanceByAccountingPeriods)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(AccountBalanceByAccountingPeriodState expectedState, AccountBalanceByAccountingPeriod entity)
    {
        Assert.Equal(expectedState.AccountingPeriodYear, entity.AccountingPeriod.Year);
        Assert.Equal(expectedState.AccountingPeriodMonth, entity.AccountingPeriod.Month);
        new FundAmountValidator(entity.StartingBalance.FundBalances).Validate(expectedState.StartingFundBalances);
        Assert.Empty(entity.StartingBalance.PendingFundBalanceChanges);
        new FundAmountValidator(entity.EndingBalance.FundBalances).Validate(expectedState.EndingFundBalances);
        new FundAmountValidator(entity.EndingBalance.PendingFundBalanceChanges).Validate(expectedState.EndingPendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an Account Balance by Accounting Period
/// </summary>
internal sealed record AccountBalanceByAccountingPeriodState
{
    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int AccountingPeriodYear { get; init; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int AccountingPeriodMonth { get; init; }

    /// <summary>
    /// Starting Fund Balances for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> StartingFundBalances { get; init; }

    /// <summary>
    /// Ending Fund Balances for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> EndingFundBalances { get; init; }

    /// <summary>
    /// Ending Pending Fund Balance Changes for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> EndingPendingFundBalanceChanges { get; init; }
}

/// <summary>
/// Comparer class that compares Account Balance by Accounting Periods
/// </summary>
internal sealed class AccountBalanceByAccountingPeriodComparer : EntityComparerBase,
    IComparer<AccountBalanceByAccountingPeriod>,
    IComparer<AccountBalanceByAccountingPeriodState>
{
    /// <inheritdoc/>
    public int Compare(AccountBalanceByAccountingPeriod? first, AccountBalanceByAccountingPeriod? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(
            new Key(first.AccountingPeriod.Year, first.AccountingPeriod.Month),
            new Key(second.AccountingPeriod.Year, second.AccountingPeriod.Month));
    }

    /// <inheritdoc/>
    public int Compare(AccountBalanceByAccountingPeriodState? first, AccountBalanceByAccountingPeriodState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(
            new Key(first.AccountingPeriodYear, first.AccountingPeriodMonth),
            new Key(second.AccountingPeriodYear, second.AccountingPeriodMonth));
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
    /// Record class representing the key used to compare an Account Balance by Accounting Period or 
    /// Account Balance by Accounting Period state
    /// </summary>
    /// <param name="Year">Year for this Accounting Period</param>
    /// <param name="Month">Month for this Accounting Period</param>
    private sealed record Key(int Year, int Month);
}