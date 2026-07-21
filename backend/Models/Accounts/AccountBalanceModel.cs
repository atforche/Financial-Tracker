namespace Models.Accounts;

/// <summary>
/// Model representing an Account Balance
/// </summary>
public class AccountBalanceModel
{
    /// <summary>
    /// Posted Balance for the Account Balance
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Pending Debit Amount for the Account Balance
    /// </summary>
    public required decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending Credit Amount for the Account Balance
    /// </summary>
    public required decimal PendingCreditAmount { get; init; }

    /// <summary>
    /// Balance including pending debit and credit amounts for the Account Balance
    /// </summary>
    public required decimal PendingBalance { get; init; }
}