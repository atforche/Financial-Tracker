using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates the provided Account Balance by Event matches the expected state
/// </summary>
internal sealed class AccountBalanceByEventValidator : EntityValidatorBase<AccountBalanceByEvent, AccountBalanceByEventState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceByEvent entity, AccountBalanceByEventState expectedState)
    {
        Assert.Equal(expectedState.AccountName, entity.BalanceEvent.Account.Name);
        Assert.Equal(expectedState.AccountingPeriodYear, entity.BalanceEvent.AccountingPeriodYear);
        Assert.Equal(expectedState.AccountingPeriodMonth, entity.BalanceEvent.AccountingPeriodMonth);
        Assert.Equal(expectedState.EventDate, entity.BalanceEvent.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.BalanceEvent.EventSequence);
        new FundAmountValidator().Validate(entity.AccountBalance.FundBalances, expectedState.FundBalances);
        new FundAmountValidator().Validate(entity.AccountBalance.PendingFundBalanceChanges, expectedState.PendingFundBalanceChanges);
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