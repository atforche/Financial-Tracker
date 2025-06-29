using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="AccountBalanceCheckpoint"/> match the expected states
/// </summary>
internal sealed class AccountBalanceCheckpointValidator : Validator<AccountBalanceCheckpoint, AccountBalanceCheckpointState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceCheckpoint entity, AccountBalanceCheckpointState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.AccountId, entity.Account.Id);
        Assert.Equal(expectedState.AccountingPeriodId, entity.AccountingPeriodId);
        new FundAmountValidator().Validate(entity.FundBalances, expectedState.FundBalances);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="AccountBalanceCheckpoint"/>
/// </summary>
internal sealed record AccountBalanceCheckpointState
{
    /// <summary>
    /// Account ID for this Account Balance Checkpoint
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Accounting Period ID for this Account Balance Checkpoint
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance Checkpoint
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }
}