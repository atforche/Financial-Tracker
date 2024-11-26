using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Account Balance Checkpoints match the expected states
/// </summary>
internal sealed class AccountBalanceCheckpointValidator : EntityValidatorBase<AccountBalanceCheckpoint,
    AccountBalanceCheckpointState,
    AccountBalanceCheckpointComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceCheckpoint">Accounting Balance Checkpoint to validate</param>
    public AccountBalanceCheckpointValidator(AccountBalanceCheckpoint accountBalanceCheckpoint)
        : this([accountBalanceCheckpoint])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountBalanceCheckpoints">Account Balance Checkpoints to validate</param>
    public AccountBalanceCheckpointValidator(IEnumerable<AccountBalanceCheckpoint> accountBalanceCheckpoints)
        : base(accountBalanceCheckpoints)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(AccountBalanceCheckpointState expectedState, AccountBalanceCheckpoint entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.Type, entity.Type);
        new FundAmountValidator(entity.FundBalances).Validate(expectedState.FundBalances.ToList());
    }
}

/// <summary>
/// Record class representing the state of an Account Balance Checkpoint
/// </summary>
internal sealed record AccountBalanceCheckpointState
{
    /// <summary>
    /// Account Name for this Account Balance Checkpoint
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Type for this Account Balance Checkpoint
    /// </summary>
    public required AccountBalanceCheckpointType Type { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance Checkpoint
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }
}

/// <summary>
/// Comparer class that compares Account Balance Checkpoints and Account Balance Checkpoint States
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal sealed class AccountBalanceCheckpointComparer : EntityComparerBase,
    IComparer<AccountBalanceCheckpoint>,
    IComparer<AccountBalanceCheckpointState>
{
    /// <inheritdoc/>
    public int Compare(AccountBalanceCheckpoint? first, AccountBalanceCheckpoint? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.Account.Name, first.Type), new Key(second.Account.Name, second.Type));
    }

    /// <inheritdoc/>
    public int Compare(AccountBalanceCheckpointState? first, AccountBalanceCheckpointState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.AccountName, first.Type), new Key(second.AccountName, second.Type));
    }

    /// <summary>
    /// Compares the provided keys to determine their ordering
    /// </summary>
    /// <param name="first">First Key to compare</param>
    /// <param name="second">Second Key to compare</param>
    /// <returns>The ordering of the provided keys</returns>
    private static int ComparePrivate(Key first, Key second)
    {
        if (!string.Equals(first.AccountName, second.AccountName, StringComparison.OrdinalIgnoreCase))
        {
            return string.Compare(first.AccountName, second.AccountName, StringComparison.OrdinalIgnoreCase);
        }
        return first.Type.CompareTo(second.Type);
    }

    /// <summary>
    /// Record class representing the key used to compare an Account Balance Checkpoint or Account Balance Checkpoint State
    /// </summary>
    /// <param name="AccountName">Account Name of the Account Balance Checkpoint</param>
    /// <param name="Type">Type of the Account Balance Checkpoint</param>
    private sealed record Key(string AccountName, AccountBalanceCheckpointType Type);
}