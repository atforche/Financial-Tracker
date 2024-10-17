namespace Domain.Entities;

/// <summary>
/// Entity class representing an Account Starting Balance
/// </summary>
/// <remarks>
/// An Account Starting Balance represents the balance of an Account at the 
/// beginning of an Accounting Period. This saved balance serves as a checkpoint 
/// to improve the efficiency of calculating the balance of an Account at any point in time.
/// </remarks>
public class AccountStartingBalance
{
    /// <summary>
    /// ID for this Account Starting Balance
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// ID of the Account for this Account Starting Balance
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// ID of the Accounting Period for this Account Starting Balance
    /// </summary>
    public Guid AccountingPeriodId { get; }

    /// <summary>
    /// Starting balance of the Account during the Accounting Period for this Account Starting Balance
    /// </summary>
    public decimal StartingBalance { get; }

    /// <summary>
    /// Reconstructs an existing Account Starting Balance
    /// </summary>
    /// <param name="request">Request to recreate an Account Starting Balance</param>
    public AccountStartingBalance(IRecreateAccountStartingBalanceRequest request)
    {
        Id = request.Id;
        AccountId = request.AccountId;
        AccountingPeriodId = request.AccountingPeriodId;
        StartingBalance = request.StartingBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="request">Request to create an Account Starting Balance</param>
    internal AccountStartingBalance(CreateAccountStartingBalanceRequest request)
    {
        Id = Guid.NewGuid();
        AccountId = request.Account.Id;
        AccountingPeriodId = request.AccountingPeriod.Id;
        StartingBalance = request.StartingBalance;
        Validate();
    }

    /// <summary>
    /// Validates the current Account Starting Balance
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountingPeriodId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (StartingBalance < 0)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Record representing a request to create an Account Starting Balance
/// </summary>
public record CreateAccountStartingBalanceRequest
{
    /// <inheritdoc cref="AccountStartingBalance.AccountId"/>
    public required Account Account { get; init; }

    /// <inheritdoc cref="AccountStartingBalance.AccountingPeriodId"/>
    public required AccountingPeriod AccountingPeriod { get; init; }

    /// <inheritdoc cref="AccountStartingBalance.StartingBalance"/>
    public required decimal StartingBalance { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Account Starting Balance
/// </summary>
public interface IRecreateAccountStartingBalanceRequest
{
    /// <inheritdoc cref="AccountStartingBalance.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="AccountStartingBalance.AccountId"/>
    Guid AccountId { get; }

    /// <inheritdoc cref="AccountStartingBalance.AccountingPeriodId"/>
    Guid AccountingPeriodId { get; }

    /// <inheritdoc cref="AccountStartingBalance.StartingBalance"/>
    decimal StartingBalance { get; }
}