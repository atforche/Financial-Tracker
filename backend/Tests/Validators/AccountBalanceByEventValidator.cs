using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that valdates the provided Account Balance by Event matches the expected state
/// </summary>
internal sealed class AccountBalanceByEventValidator : EntityValidatorBase<AccountBalanceByEvent,
    AccountBalanceByEventState, AccountBalanceByEventComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByEvent">Account Balance by Event to validate</param>
    public AccountBalanceByEventValidator(AccountBalanceByEvent accountBalanceByEvent)
        : this([accountBalanceByEvent])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByEvents">Account Balance by Events to validate</param>
    public AccountBalanceByEventValidator(IEnumerable<AccountBalanceByEvent> accountBalanceByEvents)
        : base(accountBalanceByEvents)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(AccountBalanceByEventState expectedState, AccountBalanceByEvent entity)
    {
        Assert.Equal(expectedState.AccountName, entity.BalanceEvent.Account.Name);
        Assert.Equal(expectedState.AccountingPeriodYear, entity.BalanceEvent.AccountingPeriod.Year);
        Assert.Equal(expectedState.AccountingPeriodMonth, entity.BalanceEvent.AccountingPeriod.Month);
        Assert.Equal(expectedState.EventDate, entity.BalanceEvent.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.BalanceEvent.EventSequence);
        new FundAmountValidator(entity.AccountBalance.FundBalances).Validate(expectedState.FundBalances);
        new FundAmountValidator(entity.AccountBalance.PendingFundBalanceChanges).Validate(expectedState.PendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an Account Balance by Event
/// </summary>
internal sealed record AccountBalanceByEventState
{
    /// <summary>
    /// Account Name for this Account Balance by Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Accounting Period Year for this Account Balance by Event
    /// </summary>
    public required int AccountingPeriodYear { get; init; }

    /// <summary>
    /// Accounting Period Month for this Account Balance by Event
    /// </summary>
    public required int AccountingPeriodMonth { get; init; }

    /// <summary>
    /// Event Date for this Account Balance by Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Account Balance by Event
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance by Event
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance by Event
    /// </summary>
    public required List<FundAmountState> PendingFundBalanceChanges { get; init; }
}

/// <summary>
/// Comparer class that compares Account Balance by Events
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal sealed class AccountBalanceByEventComparer : EntityComparerBase,
    IComparer<AccountBalanceByEvent>,
    IComparer<AccountBalanceByEventState>
{
    /// <inheritdoc/>
    public int Compare(AccountBalanceByEvent? first, AccountBalanceByEvent? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        if (first.BalanceEvent.AccountingPeriod.Year.CompareTo(second.BalanceEvent.AccountingPeriod.Year) != 0)
        {
            return first.BalanceEvent.AccountingPeriod.Year.CompareTo(second.BalanceEvent.AccountingPeriod.Year);
        }
        if (first.BalanceEvent.AccountingPeriod.Month.CompareTo(second.BalanceEvent.AccountingPeriod.Month) != 0)
        {
            return first.BalanceEvent.AccountingPeriod.Month.CompareTo(second.BalanceEvent.AccountingPeriod.Month);
        }
        if (first.BalanceEvent.EventDate.CompareTo(second.BalanceEvent.EventDate) != 0)
        {
            return first.BalanceEvent.EventDate.CompareTo(second.BalanceEvent.EventDate);
        }
        return first.BalanceEvent.EventSequence.CompareTo(second.BalanceEvent.EventSequence);
    }

    /// <inheritdoc/>
    public int Compare(AccountBalanceByEventState? first, AccountBalanceByEventState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        if (first.AccountingPeriodYear.CompareTo(second.AccountingPeriodYear) != 0)
        {
            return first.AccountingPeriodYear.CompareTo(second.AccountingPeriodYear);
        }
        if (first.AccountingPeriodMonth.CompareTo(second.AccountingPeriodMonth) != 0)
        {
            return first.AccountingPeriodMonth.CompareTo(second.AccountingPeriodMonth);
        }
        if (first.EventDate.CompareTo(second.EventDate) != 0)
        {
            return first.EventDate.CompareTo(second.EventDate);
        }
        return first.EventSequence.CompareTo(second.EventSequence);
    }
}