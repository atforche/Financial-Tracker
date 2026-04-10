namespace Domain.Accounts;

/// <summary>
/// Enum representing the different Account types
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Standard Account
    /// </summary>
    Standard,

    /// <summary>
    /// Credit Card Account
    /// </summary>
    CreditCard,

    /// <summary>
    /// Investment Account
    /// </summary>
    Investment,

    /// <summary>
    /// Debt Account
    /// </summary>
    Debt,

    /// <summary>
    /// Retirement Account
    /// </summary>
    Retirement,

    /// <summary>
    /// Escrow Account
    /// </summary>
    Escrow,
}

/// <summary>
/// Extension methods for AccountType
/// </summary>
public static class AccountTypeExtensions
{
    /// <summary>
    /// Determines if an AccountType is tracked
    /// </summary>
    public static bool IsTracked(this AccountType accountType) => accountType switch
    {
        AccountType.Standard => true,
        AccountType.CreditCard => true,
        AccountType.Investment => true,
        AccountType.Debt => false,
        AccountType.Retirement => false,
        AccountType.Escrow => false,
        _ => throw new ArgumentOutOfRangeException(nameof(accountType), $"Not expected account type value: {accountType}"),
    };

    /// <summary>
    /// Determines if an AccountType is a debt account
    /// </summary>
    public static bool IsDebt(this AccountType accountType) => accountType switch
    {
        AccountType.Standard => false,
        AccountType.CreditCard => true,
        AccountType.Investment => false,
        AccountType.Debt => true,
        AccountType.Retirement => false,
        AccountType.Escrow => false,
        _ => throw new ArgumentOutOfRangeException(nameof(accountType), $"Not expected account type value: {accountType}"),
    };
}