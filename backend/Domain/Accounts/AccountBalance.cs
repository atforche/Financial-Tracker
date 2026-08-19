namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Account for this Account Balance
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Posted Balance for this Account Balance
    /// </summary>
    public decimal PostedBalance { get; }

    /// <summary>
    /// Balance after current unposted Transaction effects are applied.
    /// </summary>
    public decimal BalanceIncludingPending { get; }

    /// <summary>
    /// Debits the specified amount from this Account Balance.
    /// </summary>
    internal AccountBalance Debit(decimal amount) => new(Account, PostedBalance + (Account.Type.IsDebt() ? amount : -amount));

    /// <summary>
    /// Credits the specified amount to this Account Balance.
    /// </summary>
    internal AccountBalance Credit(decimal amount) => new(Account, PostedBalance + (Account.Type.IsDebt() ? -amount : amount));

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(Account account, decimal postedBalance, decimal? balanceIncludingPending = null)
    {
        Account = account;
        PostedBalance = postedBalance;
        BalanceIncludingPending = balanceIncludingPending ?? postedBalance;
    }
}
