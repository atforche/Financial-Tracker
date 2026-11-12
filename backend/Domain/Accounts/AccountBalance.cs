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
    /// Net change from unposted Transactions, which is not included in the Posted Balance.
    /// </summary>
    public decimal PendingAmount { get; }

    /// <summary>
    /// Debits the specified amount from this Account Balance.
    /// </summary>
    internal AccountBalance Debit(decimal amount) => new(Account, PostedBalance + (Account.Type.IsDebt() ? amount : -amount));

    /// <summary>
    /// Adds the specified signed amount to this Account Balance as it is displayed, regardless of the Account type.
    /// </summary>
    internal AccountBalance Adjust(decimal amount) => new(Account, PostedBalance + amount);

    /// <summary>
    /// Credits the specified amount to this Account Balance.
    /// </summary>
    internal AccountBalance Credit(decimal amount) => new(Account, PostedBalance + (Account.Type.IsDebt() ? -amount : amount));

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(Account account, decimal postedBalance, decimal pendingAmount = 0)
    {
        Account = account;
        PostedBalance = postedBalance;
        PendingAmount = pendingAmount;
    }
}
