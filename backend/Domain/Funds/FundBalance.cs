namespace Domain.Funds;

/// <summary>
/// Value object class representing the balance of a Fund
/// </summary>
public class FundBalance
{
    /// <summary>
    /// Fund for this Fund Balance
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Posted Balance for this Fund Balance
    /// </summary>
    public decimal PostedBalance { get; }

    /// <summary>
    /// Net change from unposted Transactions, which is not included in the Posted Balance.
    /// </summary>
    public decimal PendingAmount { get; }

    /// <summary>
    /// Debits the specified amount from this Fund Balance.
    /// </summary>
    internal FundBalance Debit(decimal amount) => new(Fund, PostedBalance - amount);

    /// <summary>
    /// Credits the specified amount to this Fund Balance.
    /// </summary>
    internal FundBalance Credit(decimal amount) => new(Fund, PostedBalance + amount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(Fund fund, decimal postedBalance, decimal pendingAmount = 0)
    {
        Fund = fund;
        PostedBalance = postedBalance;
        PendingAmount = pendingAmount;
    }
}
