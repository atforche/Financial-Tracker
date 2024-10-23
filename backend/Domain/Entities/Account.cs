namespace Domain.Entities;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held 
/// in and transfered from.
/// </remarks>
public class Account
{
    /// <summary>
    /// ID for this Account
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; }

    /// <summary>
    /// Reconstructs an existing Account
    /// </summary>
    /// <param name="request">Request to recreate an Account</param>
    public Account(IRecreateAccountRequest request)
    {
        Id = request.Id;
        Name = request.Name;
        Type = request.Type;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    internal Account(string name, AccountType type)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Validate();
    }

    /// <summary>
    /// Validates the current Account
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (string.IsNullOrEmpty(Name))
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Enum representing the different Account types
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Standard Account
    /// </summary>
    /// <remarks>
    /// A Standard Account represents a standard checking or savings account.
    /// Debiting a Standard Account will decrease its balance and crediting a
    /// Standard Account will increase its balance.
    /// </remarks>
    Standard,

    /// <summary>
    /// Debt Account
    /// </summary>
    /// <remarks>
    /// A Debt Account represents a credit card or loan account.
    /// Debiting a Debt Account will increase its balance and crediting a 
    /// Debt Account will decrease its balance.
    /// </remarks>
    Debt,

    /// <summary>
    /// Investment Account
    /// </summary>
    /// <remarks>
    /// An Investment Account represents a retirement or brokerage account.
    /// Investment Accounts are similar to Standard Accounts, however they
    /// also experience periodic changes in value as the value of the assets
    /// in the Account change.
    /// </remarks>
    Investment,
}

/// <summary>
/// Interface representing a request to recreate an existing Account
/// </summary>
public interface IRecreateAccountRequest
{
    /// <inheritdoc cref="Account.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="Account.Name"/>
    string Name { get; }

    /// <inheritdoc cref="Account.Type"/>
    AccountType Type { get; }
}