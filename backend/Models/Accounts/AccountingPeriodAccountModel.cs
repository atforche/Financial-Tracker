namespace Models.Accounts;

/// <summary>
/// Model representing an Account in the context of a specific Accounting Period
/// </summary>
public class AccountingPeriodAccountModel
{
    /// <summary>
    /// ID for the Account
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account
    /// </summary>
    public required AccountTypeModel Type { get; init; }

    /// <summary>
    /// Opening balance for the Account
    /// </summary>
    public required AccountBalanceModel OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance for the Account
    /// </summary>
    public required AccountBalanceModel ClosingBalance { get; init; }
}