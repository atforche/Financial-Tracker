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
}
