namespace Domain.ValueObjects;

/// <summary>
/// Balance of an Account
/// </summary>
/// <remarks>
/// A Transaction is considered pending against an Account in the following situations:
/// 1. The Transaction has not been posted yet
/// 2. The Transaction has been posted, however the period in time of this balance falls between the 
///     Transaction's accounting date and statement date
/// </remarks>
public class AccountBalance
{
    /// <summary>
    /// Current balance for each Fund within the Account
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances { get; }

    /// <summary>
    /// Current balance for each Fund within the Account, including pending Transactions
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalancesIncludingPendingTransactions { get; }

    /// <summary>
    /// Balance of the Account
    /// </summary>
    public decimal Balance => FundBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Balance of the Account including pending Transactions
    /// </summary>
    public decimal BalanceIncludingPendingTransactions => FundBalancesIncludingPendingTransactions.Sum(balance => balance.Amount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundBalances">List of balances for each Fund within the Account</param>
    /// <param name="fundBalancesIncludingPendingTransactions">List of balances for each Fund within the Account, including pending Transactions</param>
    internal AccountBalance(IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> fundBalancesIncludingPendingTransactions)
    {
        FundBalances = fundBalances.ToList();
        FundBalancesIncludingPendingTransactions = fundBalancesIncludingPendingTransactions.ToList();
    }
}