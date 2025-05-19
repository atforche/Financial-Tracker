using Domain.AccountingPeriods;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountingPeriod"/> matches the expected state
/// </summary>
internal sealed class AccountingPeriodValidator : EntityValidator<AccountingPeriod, AccountingPeriodState>
{
    /// <inheritdoc/>
    public override void Validate(AccountingPeriod entity, AccountingPeriodState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.Key, entity.Key);
        Assert.Equal(expectedState.IsOpen, entity.IsOpen);
        new TransactionValidator().Validate(entity.Transactions, expectedState.Transactions);
        new FundConversionValidator().Validate(entity.FundConversions, expectedState.FundConversions);
        new ChangeInValueValidator().Validate(entity.ChangeInValues, expectedState.ChangeInValues);
        new AccountAddedBalanceEventValidator().Validate(entity.AccountAddedBalanceEvents, expectedState.AccountAddedBalanceEvents);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountingPeriod"/>
/// </summary>
internal sealed record AccountingPeriodState
{
    /// <summary>
    /// Key for this Accounting Period
    /// </summary>
    public required AccountingPeriodKey Key { get; init; }

    /// <summary>
    /// Is Open Flag for this Accounting Period
    /// </summary>
    public required bool IsOpen { get; init; }

    /// <summary>
    /// Transactions for this Accounting Period
    /// </summary>
    public required List<TransactionState> Transactions { get; init; }

    /// <summary>
    /// Fund Conversion for this Accounting Period
    /// </summary>
    public required List<FundConversionState> FundConversions { get; init; }

    /// <summary>
    /// Change in Values for this Accounting Period
    /// </summary>
    public required List<ChangeInValueState> ChangeInValues { get; init; }

    /// <summary>
    /// Account Added Balance Events for this Accounting Period
    /// </summary>
    public required List<AccountAddedBalanceEventState> AccountAddedBalanceEvents { get; init; }
}