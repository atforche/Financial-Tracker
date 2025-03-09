using Domain.Aggregates.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Transaction matches the expected state
/// </summary>
internal sealed class TransactionValidator : EntityValidatorBase<Transaction, TransactionState, TransactionComparer>
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Transaction to validate</param>
    public TransactionValidator(Transaction transaction)
        : this([transaction])
    {
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transactions">Transactions to validate</param>
    public TransactionValidator(IEnumerable<Transaction> transactions)
        : base(transactions)
    {
    }

    /// <inheritdoc/>
    protected override void ValidatePrivate(TransactionState expectedState, Transaction entity)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.TransactionDate, entity.TransactionDate);
        new FundAmountValidator(entity.AccountingEntries).Validate(expectedState.AccountingEntries);
        new TransactionBalanceEventValidator(entity.TransactionBalanceEvents).Validate(expectedState.TransactionBalanceEvents);
    }
}

/// <summary>
/// Record class representing the state of a Transaction
/// </summary>
internal sealed record TransactionState
{
    /// <summary>
    /// Transaction Date for this Transaction
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Accounting Entries for this Transaction
    /// </summary>
    public required List<FundAmountState> AccountingEntries { get; init; }

    /// <summary>
    /// Transaction Balance Events for this Transaction
    /// </summary>
    public required List<TransactionBalanceEventState> TransactionBalanceEvents { get; init; }
}

/// <summary>
/// Comparer class that compares Transactions and Transaction States
/// </summary>
internal sealed class TransactionComparer : EntityComparerBase, IComparer<Transaction>, IComparer<TransactionState>
{
    /// <inheritdoc/>
    public int Compare(Transaction? first, Transaction? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.TransactionDate), new Key(second.TransactionDate));
    }

    /// <inheritdoc/>
    public int Compare(TransactionState? first, TransactionState? second)
    {
        if (TryCompareNull(first, second, out int? result))
        {
            return result.Value;
        }
        return ComparePrivate(new Key(first.TransactionDate), new Key(second.TransactionDate));
    }

    /// <summary>
    /// Compares the provided keys to determine their ordering
    /// </summary>
    /// <param name="first">First Key to compare</param>
    /// <param name="second">Second Key to compare</param>
    /// <returns>The ordering of the provided keys</returns>
    private static int ComparePrivate(Key first, Key second) => first.TransactionDate.CompareTo(second.TransactionDate);

    /// <summary>
    /// Record class representing the key used to compare a Transaction or Transaction State
    /// </summary>
    /// <param name="TransactionDate">Transaction Date for this Transaction</param>
    private sealed record Key(DateOnly TransactionDate);
}