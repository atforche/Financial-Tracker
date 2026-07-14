namespace Models.Funds;

/// <summary>
/// Model representing an Fund Balance
/// </summary>
public class FundBalanceModel
{
    /// <summary>
    /// Posted Balance for the Fund Balance
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Pending Debit Amount for the Fund Balance
    /// </summary>
    public required decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending Credit Amount for the Fund Balance
    /// </summary>
    public required decimal PendingCreditAmount { get; init; }
}