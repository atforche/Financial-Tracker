using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountAddedBalanceEvent"/> matches the expected state
/// </summary>
internal sealed class AccountAddedBalanceEventValidator : EntityValidator<AccountAddedBalanceEvent, AccountAddedBalanceEventState>
{
    /// <inheritdoc/>
    public override void Validate(AccountAddedBalanceEvent entityToValidate, AccountAddedBalanceEventState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entityToValidate.Id.ExternalId);
        Assert.Equal(expectedState.AccountingPeriodKey, entityToValidate.AccountingPeriodKey);
        Assert.Equal(expectedState.AccountName, entityToValidate.Account.Name);
        Assert.Equal(expectedState.EventDate, entityToValidate.EventDate);
        Assert.Equal(expectedState.EventSequence, entityToValidate.EventSequence);
        new FundAmountValidator().Validate(entityToValidate.FundAmounts, expectedState.FundAmounts);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountAddedBalanceEvent"/>
/// </summary>
internal sealed record AccountAddedBalanceEventState
{
    /// <summary>
    /// Accounting Period Key for this Account Added Balance Event
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Account for this Account Added Balance Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Event Date for this Account Added Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Event Sequence for this Account Added Balance Event
    /// </summary>
    public required int EventSequence { get; init; }

    /// <summary>
    /// Fund Amounts for this Account Added Balance Event
    /// </summary>
    public required List<FundAmountState> FundAmounts { get; init; }
}