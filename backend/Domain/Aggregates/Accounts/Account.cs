using Domain.ValueObjects;

namespace Domain.Aggregates.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account : EntityBase
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    internal Account(string name, AccountType type)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        Validate();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base(new EntityId(default, Guid.NewGuid())) => Name = "";

    /// <summary>
    /// Validates the current Account
    /// </summary>
    private void Validate()
    {
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