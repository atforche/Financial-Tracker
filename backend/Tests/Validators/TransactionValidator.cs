using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="Transaction"/> matches the expected state
/// </summary>
internal sealed class TransactionValidator : Validator<Transaction, TransactionState>
{
    /// <inheritdoc/>
    public override void Validate(Transaction entity, TransactionState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.AccountingPeriodId, entity.AccountingPeriodId);
        Assert.Equal(expectedState.Date, entity.Date);
        Assert.Equal(expectedState.DebitAccountId, entity.DebitAccountId);
        new FundAmountValidator().Validate(entity.DebitFundAmounts ?? [], expectedState.DebitFundAmounts ?? []);
        Assert.Equal(expectedState.CreditAccountId, entity.CreditAccountId);
        new FundAmountValidator().Validate(entity.CreditFundAmounts ?? [], expectedState.CreditFundAmounts ?? []);
        new TransactionBalanceEventValidator().Validate(entity.TransactionBalanceEvents, expectedState.TransactionBalanceEvents);
    }
}

/// <summary>
/// Record class representing the state of a <see cref="Transaction"/>
/// </summary>
internal sealed record TransactionState
{
    /// <summary>
    /// Accounting Period ID for this Transaction Balance Event
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Debit Account ID for this Transaction
    /// </summary>
    public AccountId? DebitAccountId { get; init; }

    /// <summary>
    /// Debit Fund Amounts for this Transaction
    /// </summary>
    public List<FundAmountState>? DebitFundAmounts { get; init; }

    /// <summary>
    /// Credit Account ID for this Transaction
    /// </summary>
    public AccountId? CreditAccountId { get; init; }

    /// <summary>
    /// Credit Fund Amounts for this Transaction
    /// </summary>
    public List<FundAmountState>? CreditFundAmounts { get; init; }

    /// <summary>
    /// Transaction Balance Events for this Transaction
    /// </summary>
    public required List<TransactionBalanceEventState> TransactionBalanceEvents { get; init; }
}