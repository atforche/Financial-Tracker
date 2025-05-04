using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Account Balance by Accounting Period matches the expected state
/// </summary>
internal sealed class AccountBalanceByAccountingPeriodValidator : EntityValidator<AccountBalanceByAccountingPeriod, AccountBalanceByAccountingPeriodState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceByAccountingPeriod entityToValidate, AccountBalanceByAccountingPeriodState expectedState)
    {
        Assert.Equal(expectedState.AccountingPeriodKey, entityToValidate.AccountingPeriod.Key);
        new FundAmountValidator().Validate(entityToValidate.StartingBalance.FundBalances, expectedState.StartingFundBalances);
        Assert.Empty(entityToValidate.StartingBalance.PendingFundBalanceChanges);
        new FundAmountValidator().Validate(entityToValidate.EndingBalance.FundBalances, expectedState.EndingFundBalances);
        new FundAmountValidator().Validate(entityToValidate.EndingBalance.PendingFundBalanceChanges, expectedState.EndingPendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an Account Balance by Accounting Period
/// </summary>
internal sealed record AccountBalanceByAccountingPeriodState
{
    /// <summary>
    /// Key for this Accounting Period
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Starting Fund Balances for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> StartingFundBalances { get; init; }

    /// <summary>
    /// Ending Fund Balances for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> EndingFundBalances { get; init; }

    /// <summary>
    /// Ending Pending Fund Balance Changes for this Account Balance by Accounting Period
    /// </summary>
    public required List<FundAmountState> EndingPendingFundBalanceChanges { get; init; }
}