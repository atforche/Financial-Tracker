using Domain.Accounts;

namespace Domain.Transactions.Refunds;

/// <summary>
/// Tracked account destination of a refund.
/// </summary>
public class RefundTransactionDestination
{
    /// <summary>
    /// The tracked account destination of the refund transaction.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// The posted date of the refund transaction for the destination account.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    public RefundTransactionDestination(Account account, DateOnly? postedDate)
    {
        Account = account;
        PostedDate = postedDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private RefundTransactionDestination() { Account = null!; }
}
