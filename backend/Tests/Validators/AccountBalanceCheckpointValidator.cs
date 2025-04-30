using Domain.Aggregates.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided Account Balance Checkpoints match the expected states
/// </summary>
internal sealed class AccountBalanceCheckpointValidator : EntityValidator<AccountBalanceCheckpoint, AccountBalanceCheckpointState>
{
    /// <inheritdoc/>
    public override void Validate(AccountBalanceCheckpoint entity, AccountBalanceCheckpointState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.ExternalId);
        Assert.Equal(expectedState.AccountName, entity.Account.Name);
        new FundAmountValidator().Validate(entity.FundBalances, expectedState.FundBalances);
    }
}

/// <summary>
/// Record class representing the state of an Account Balance Checkpoint
/// </summary>
internal sealed record AccountBalanceCheckpointState
{
    /// <summary>
    /// Account Name for this Account Balance Checkpoint
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Accounting Period Year for this Account Balance Checkpoint
    /// </summary>
    public required int AccountingPeriodYear { get; init; }

    /// <summary>
    /// Accounting Period Month for this Account Balance Checkpoint
    /// </summary>
    public required int AccountingPeriodMonth { get; init; }

    /// <summary>
    /// Fund Balances for this Account Balance Checkpoint
    /// </summary>
    public required List<FundAmountState> FundBalances { get; init; }
}