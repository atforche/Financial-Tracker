namespace Models.Funds;

/// <summary>
/// Model representing an Fund Balance
/// </summary>
public class FundBalanceModel
{
    /// <summary>
    /// Fund ID for the Fund Balance
    /// </summary>
    public required Guid FundId { get; init; }

    /// <summary>
    /// Fund Name for the Fund Balance
    /// </summary>
    public required string FundName { get; init; }

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