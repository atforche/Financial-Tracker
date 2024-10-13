namespace RestApi.Models.AccountBalance;

/// <summary>
/// REST model representing an account balance for a particular date
/// </summary>
public class AccountBalanceByDateModel
{
    /// <summary>
    /// Date for this account balance
    /// </summary>
    public required DateOnly Date { get; set; }

    /// <summary>
    /// Balance for this account balance
    /// </summary>
    public required decimal Balance { get; set; }

    /// <summary>
    /// Balance including pending transactions for this account balance
    /// </summary>
    public required decimal BalanceIncludingPendingTransactions { get; set; }
}