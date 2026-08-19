namespace Models.Accounts;

/// <summary>
/// Model representing an Account with a balance range.
/// </summary>
public class AccountWithBalanceRangeModel : AccountModel
{
    /// <summary>
    /// Balance at the beginning of the requested range.
    /// </summary>
    public required decimal StartingBalance { get; init; }

    /// <summary>
    /// Balance at the end of the requested range.
    /// </summary>
    public required decimal EndingBalance { get; init; }
}
