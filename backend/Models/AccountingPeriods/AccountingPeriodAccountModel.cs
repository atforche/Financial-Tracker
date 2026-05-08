using Models.Accounts;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Account in the context of a specific Accounting Period
/// </summary>
public class AccountingPeriodAccountModel : AccountModel
{
    /// <summary>
    /// Opening balance for the Account
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance for the Account
    /// </summary>
    public required decimal ClosingBalance { get; init; }
}