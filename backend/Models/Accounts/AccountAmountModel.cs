namespace Models.Accounts;

/// <summary>
/// Model representing an amount associated with a particular Account
/// </summary>
public class AccountAmountModel
{
    /// <summary>
    /// Account for this Account Amount
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Name of the Account for this Account Amount
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Amount for this Account Amount
    /// </summary>
    public required decimal Amount { get; init; }
}