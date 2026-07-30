using Domain.Funds;

namespace Domain.Transactions.Funds;

/// <summary>
/// Value object representing a destination of money for a fund transaction.
/// </summary>
public class FundTransactionDestination
{
    /// <summary>
    /// Fund for this fund transaction destination.
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Amount for this fund transaction destination.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public FundTransactionDestination(Fund fund, decimal amount)
    {
        Fund = fund;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundTransactionDestination()
    {
        Fund = null!;
    }
}