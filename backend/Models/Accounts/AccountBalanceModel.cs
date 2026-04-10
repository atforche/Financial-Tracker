namespace Models.Accounts;

/// <summary>
/// Model representing an Account Balance
/// </summary>
public class AccountBalanceModel
{
    /// <summary>
    /// Account ID for the Account Balance
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Posted Balance for the Account Balance
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Available to Spend Balance for the Account Balance
    /// </summary>
    public required decimal? AvailableToSpend { get; init; }

    /// <summary>
    /// Pending Debit Amount for the Account Balance
    /// </summary>
    public required decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending Credit Amount for the Account Balance
    /// </summary>
    public required decimal PendingCreditAmount { get; init; }
}