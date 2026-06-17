namespace Models.Accounts;

/// <summary>
/// Model representing an Account balance on a specific date on the trends.
/// </summary>
public class AccountTrendsDateModel
{
    /// <summary>
    /// Date for the balance.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Balance for the Account on the specified date.
    /// </summary>
    public required decimal Balance { get; init; }
}