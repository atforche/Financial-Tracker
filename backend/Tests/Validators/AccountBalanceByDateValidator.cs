using Domain.ValueObjects;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountBalanceByDate"/> matches the expected state
/// </summary>
internal sealed class AccountBalanceByDateValidator : EntityValidator<AccountBalanceByDate, AccountBalanceByDateState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceByDate entity, AccountBalanceByDateState expectedState)
    {
        Assert.Equal(expectedState.Date, entity.Date);
        new FundAmountValidator().Validate(entity.AccountBalance.FundBalances, expectedState.FundBalances);
        new FundAmountValidator().Validate(entity.AccountBalance.PendingFundBalanceChanges, expectedState.PendingFundBalanceChanges);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountBalanceByDate"/>
/// </summary>
internal sealed record AccountBalanceByDateState
{
    /// <summary>
    /// Date for this Account Balance by Date
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance by Date
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance by Date
    /// </summary>
    public required List<FundAmountState> PendingFundBalanceChanges { get; init; }
}