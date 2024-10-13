using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of an Account Starting Balance
/// </summary>
public interface IAccountStartingBalanceFactory
{
    /// <summary>
    /// Creates a new Account Starting Balance with the provided properties
    /// </summary>
    /// <param name="account">Account the starting balance is for</param>
    /// <param name="request">Request to create an Account Starting Balance</param>
    /// <returns>The newly created Account Starting Balance</returns>
    AccountStartingBalance Create(Account account, CreateAccountStartingBalanceRequest request);

    /// <summary>
    /// Recreates an existing Account Starting Balance with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate an Account Starting Balance</param>
    /// <returns>The recreated Account Starting Balance</returns>
    AccountStartingBalance Recreate(IRecreateAccountStartingBalanceRequest request);
}

/// <summary>
/// Record representing a request to create an Account Starting Balance
/// </summary>
public record CreateAccountStartingBalanceRequest
{
    /// <see cref="AccountStartingBalance.AccountingPeriodId"/>
    public required AccountingPeriod AccountingPeriod { get; init; }

    /// <see cref="AccountStartingBalance.StartingBalance"/>
    public required decimal StartingBalance { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Account Starting Balance
/// </summary>
public interface IRecreateAccountStartingBalanceRequest
{
    /// <summary>
    /// ID for this Account Starting Balance
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Account ID for this Account Starting Balance
    /// </summary>
    Guid AccountId { get; }

    /// <summary>
    /// Accounting Period ID for this Account Starting Balance
    /// </summary>
    Guid AccountingPeriodId { get; }

    /// <summary>
    /// Starting Balance for this Account Starting Balance
    /// </summary>
    decimal StartingBalance { get; }
}