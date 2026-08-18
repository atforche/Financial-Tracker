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
    /// Balance after current unposted Transaction effects are applied.
    /// </summary>
    public decimal BalanceIncludingPending { get; }

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
    internal FundBalance(Fund fund, decimal postedBalance, decimal? balanceIncludingPending = null)
    {
        Fund = fund;
        PostedBalance = postedBalance;
        BalanceIncludingPending = balanceIncludingPending ?? postedBalance;
    }
}
