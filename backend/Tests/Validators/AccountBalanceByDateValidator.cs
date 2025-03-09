using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Account Balance by Date matches the expected state
/// </summary>
internal sealed class AccountBalanceByDateValidator(IEnumerable<AccountBalanceByDate> accountBalanceByDates) :
    EntityValidatorBase<AccountBalanceByDate, AccountBalanceByDateState, AccountBalanceByDateComparer>(accountBalanceByDates)
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceByDate">Account Balance by Date to validate</param>
    public AccountBalanceByDateValidator(AccountBalanceByDate accountBalanceByDate)
        : this([accountBalanceByDate])
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(AccountBalanceByDateState expectedState, AccountBalanceByDate entity)
    {
        Assert.Equal(expectedState.Date, entity.Date);
        new FundAmountValidator(entity.AccountBalance.FundBalances).Validate(expectedState.FundBalances);
        new FundAmountValidator(entity.AccountBalance.PendingFundBalanceChanges).Validate(expectedState.PendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an Account Balance by Date
/// </summary>
internal sealed record AccountBalanceByDateState
{
    /// <summary>
    /// Date for this Account Balance by Date
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance by Date
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance by Date
    /// </summary>
    public required List<FundAmountState> PendingFundBalanceChanges { get; init; }
}

/// <summary>
/// Comparer class that compares Account Balance by Dates
/// </summary>
internal sealed class AccountBalanceByDateComparer : EntityComparerBase,
    IComparer<AccountBalanceByDate>,
    IComparer<AccountBalanceByDateState>
{
    /// <inheritdoc/>
    public int Compare(AccountBalanceByDate? first, AccountBalanceByDate? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return first.Date.CompareTo(second.Date);
    }

    /// <inheritdoc/>
    public int Compare(AccountBalanceByDateState? first, AccountBalanceByDateState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return first.Date.CompareTo(second.Date);
    }
}