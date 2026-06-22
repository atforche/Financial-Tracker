using Domain.Accounts;

namespace Domain.Transactions.Spending;

/// <summary>
/// Value object representing the source of money for a spending transaction.
/// </summary>
public class SpendingTransactionSource
{
    /// <summary>
    /// Account of the spending transaction source.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Posted Date of the spending transaction source.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public SpendingTransactionSource(Account account, DateOnly? postedDate)
    {
        Account = account;
        PostedDate = postedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private SpendingTransactionSource()
    {
        Account = null!;
    }
}