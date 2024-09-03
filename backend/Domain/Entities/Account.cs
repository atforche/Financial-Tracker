namespace Domain.Entities;

/// <summary>
/// Entity class representing an Account
/// </summary>
public class Account
{
    /// <summary>
    /// Id for this Account
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; }

    /// <summary>
    /// Is active flag for this Account
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Constructs a new instance of this class 
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    /// <param name="isActive">Is active flag for this Account</param>
    public Account(string name, AccountType type, bool isActive)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        IsActive = isActive;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this account</param>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    /// <param name="isActive">Is active flag for this Account</param>
    public Account(Guid id, string name, AccountType type, bool isActive)
    {
        Id = id;
        Name = name;
        Type = type;
        IsActive = isActive;
    }
}

/// <summary>
/// Enum representing the different account types.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A Standard account represents a standard checking or savings account.
    /// </summary>
    Standard,

    /// <summary>
    /// A Debt account represents a credit card or loan account.
    /// </summary>
    Debt,

    /// <summary>
    /// An Investment account represents a retirement or brokerage account.
    /// </summary>
    Investment,
}