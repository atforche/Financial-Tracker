namespace Models.Accounts;

/// <summary>
/// Model representing the total posted balance for an Account Type.
/// </summary>
public class AccountTypeBalanceModel
{
    /// <summary>
    /// Account Type for this balance total.
    /// </summary>
    public required AccountTypeModel AccountType { get; init; }

    /// <summary>
    /// Total posted balance for the Account Type.
    /// </summary>
    public required decimal TotalBalance { get; init; }
}