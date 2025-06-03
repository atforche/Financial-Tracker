using Domain.Accounts;

namespace Tests.Validators;

/// <summary>
/// Validator class that validates that the provided <see cref="Account"/> matches the expected state
/// </summary>
internal sealed class AccountValidator : EntityValidator<Account, AccountState>
{
    /// <inheritdoc/>
    public override void Validate(Account entity, AccountState expectedState)
    {
        Assert.NotEqual(Guid.Empty, entity.Id.Value);
        Assert.Equal(expectedState.Name, entity.Name);
        Assert.Equal(expectedState.Type, entity.Type);
        new AccountAddedBalanceEventValidator().Validate(entity.AccountAddedBalanceEvent, expectedState.AccountAddedBalanceEvent);
        new AccountBalanceCheckpointValidator().Validate(entity.AccountBalanceCheckpoints, expectedState.AccountBalanceCheckpoints);
    }
}

/// <summary>
/// Record class representing the state of an <see cref="Account"/>
/// </summary>
internal sealed record AccountState
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Account Added Balance Event for this Account
    /// </summary>
    public required AccountAddedBalanceEventState AccountAddedBalanceEvent { get; init; }

    /// <summary>
    /// Account Balance Checkpoints for this Account
    /// </summary>
    public required List<AccountBalanceCheckpointState> AccountBalanceCheckpoints { get; init; }
}