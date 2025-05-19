using Domain.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="Transaction"/> matches the expected state
/// </summary>
internal sealed class TransactionValidator : EntityValidator<Transaction, TransactionState>
{
    /// <inheritdoc/>
    public override void Validate(Transaction entity, TransactionState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.TransactionDate, entity.TransactionDate);
        new FundAmountValidator().Validate(entity.AccountingEntries, expectedState.AccountingEntries);
        new TransactionBalanceEventValidator().Validate(entity.TransactionBalanceEvents, expectedState.TransactionBalanceEvents);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="Transaction"/>
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