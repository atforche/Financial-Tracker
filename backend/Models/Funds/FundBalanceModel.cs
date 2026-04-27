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
    /// Amount Assigned for the Fund Balance
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Pending Amount Assigned for the Fund Balance
    /// </summary>
    public required decimal PendingAmountAssigned { get; init; }

    /// <summary>
    /// Amount Spent for the Fund Balance
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Pending Amount Spent for the Fund Balance
    /// </summary>
    public required decimal PendingAmountSpent { get; init; }
}