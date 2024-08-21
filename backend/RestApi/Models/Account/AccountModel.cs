namespace RestApi.Models.Account;

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

/// <summary>
/// Rest model representing an Account.
/// </summary>
public class AccountModel
{
    /// <summary>
    /// Id for this Account.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Name for this Account.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Type for this Account.
    /// </summary>
    public required AccountType Type { get; set; }

    /// <summary>
    /// Is Active flag for this Account.
    /// </summary>
    public required bool IsActive { get; set; }
}