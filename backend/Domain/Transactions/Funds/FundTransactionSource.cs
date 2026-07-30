using Domain.Funds;

namespace Domain.Transactions.Funds;

/// <summary>
/// Value object representing the source of money for a fund transaction.
/// </summary>
public class FundTransactionSource
{
    /// <summary>
    /// Fund for this fund transaction source.
    /// </summary>
    public Fund Fund { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public FundTransactionSource(Fund fund)
    {
        Fund = fund;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundTransactionSource()
    {
        Fund = null!;
    }
}