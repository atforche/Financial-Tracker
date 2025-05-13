using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates the provided <see cref="AccountBalanceByEvent"/> matches the expected state
/// </summary>
internal sealed class AccountBalanceByEventValidator : EntityValidator<AccountBalanceByEvent, AccountBalanceByEventState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceByEvent entity, AccountBalanceByEventState expectedState)
    {
        Assert.Equal(expectedState.AccountingPeriodKey, entity.BalanceEvent.AccountingPeriodKey);
        Assert.Equal(expectedState.AccountName, entity.BalanceEvent.Account.Name);
        Assert.Equal(expectedState.EventDate, entity.BalanceEvent.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.BalanceEvent.EventSequence);
        new FundAmountValidator().Validate(entity.AccountBalance.FundBalances, expectedState.FundBalances);
        new FundAmountValidator().Validate(entity.AccountBalance.PendingFundBalanceChanges, expectedState.PendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountBalanceByEvent"/>
/// </summary>
internal sealed record AccountBalanceByEventState
{
    /// <summary>
    /// Accounting Period Key for this Account Balance by Event
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Account Name for this Account Balance by Event
    /// </summary>
    public required string AccountName { get; init; }

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