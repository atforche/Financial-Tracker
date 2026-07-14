namespace Models.Accounts;

/// <summary>
/// Model representing an Account along with its current balance.
/// </summary>
public class AccountWithBalanceModel : AccountModel
{
    /// <summary>
    /// Current Balance for the Account along with its details.
    /// </summary>
    public required AccountBalanceModel CurrentBalance { get; init; }
}