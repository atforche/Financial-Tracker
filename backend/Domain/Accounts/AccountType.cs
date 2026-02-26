namespace Domain.Accounts;

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