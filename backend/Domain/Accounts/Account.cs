using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account : Entity<AccountId>
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Initial Transaction for this Account
    /// </summary>
    /// <remarks>This will only be null when we're creating a new Account and the initial Transaction has not yet been created.</remarks>
    public TransactionId? InitialTransaction { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    internal Account(string name, AccountType type)
        : base(new AccountId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base() => Name = "";
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
}