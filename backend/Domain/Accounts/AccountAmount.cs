namespace Domain.Accounts;

/// <summary>
/// Value object class representing an Account Amount.
/// An Account Amount represents a monetary amount associated with a particular Account.
/// </summary>
public class AccountAmount
{
    /// <summary>
    /// Account ID for this Account Amount
    /// </summary>
    public required AccountId AccountId { get; init; }

    /// <summary>
    /// Amount for this Account Amount
    /// </summary>
    public required decimal Amount { get; init; }
}