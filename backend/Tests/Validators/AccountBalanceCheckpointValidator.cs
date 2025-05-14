using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountBalanceCheckpoint"/> match the expected states
/// </summary>
internal sealed class AccountBalanceCheckpointValidator : EntityValidator<AccountBalanceCheckpoint, AccountBalanceCheckpointState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceCheckpoint entity, AccountBalanceCheckpointState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        Assert.Equal(expectedState.AccountingPeriodKey, entity.AccountingPeriodKey);
        new FundAmountValidator().Validate(entity.FundBalances, expectedState.FundBalances);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountBalanceCheckpoint"/>
/// </summary>
internal sealed record AccountBalanceCheckpointState
{
    /// <summary>
    /// Account Name for this Account Balance Checkpoint
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Accounting Period Key for this Account Balance Checkpoint
    /// </summary>
    public required AccountingPeriodKey AccountingPeriodKey { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance Checkpoint
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }
}