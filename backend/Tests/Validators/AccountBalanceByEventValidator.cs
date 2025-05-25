using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates the provided <see cref="AccountBalanceByEvent"/> matches the expected state
/// </summary>
internal sealed class AccountBalanceByEventValidator : EntityValidator<AccountBalanceByEvent, AccountBalanceByEventState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceByEvent entity, AccountBalanceByEventState expectedState)
    {
        Assert.Equal(expectedState.AccountingPeriodId, entity.BalanceEvent.AccountingPeriodId);
        Assert.Equal(expectedState.EventDate, entity.BalanceEvent.EventDate);
        Assert.Equal(expectedState.EventSequence, entity.BalanceEvent.EventSequence);
        Assert.Equal(expectedState.AccountName, entity.BalanceEvent.Account.Name);
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
    /// Accounting Period ID for this Account Balance by Event
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Event Date for this Account Balance by Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Account Balance by Event
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Account Name for this Account Balance by Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance by Event
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance by Event
    /// </summary>
    public required List<FundAmountState> PendingFundBalanceChanges { get; init; }
}