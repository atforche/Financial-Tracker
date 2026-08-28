using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;

namespace Domain.Transactions.Refunds;

/// <summary>
/// Value object representing the source of money for a refund transaction.
/// </summary>
public class RefundTransactionSource
{
    private readonly List<FundAmount> _fundAssignments = [];

    /// <summary>
    /// Account of the refund transaction source.
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted date of the refund transaction source.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the refund transaction source.
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Amount of the refund transaction source.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Fund assignments of the refund transaction source.
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    public RefundTransactionSource(Account? account, DateOnly? postedDate, Location? location, decimal amount, IEnumerable<FundAmount> fundAssignments)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
        Amount = amount;
        _fundAssignments.AddRange(fundAssignments);
    }

    /// <summary>
    /// Constructs a new default instance of this class.
    /// </summary>
    private RefundTransactionSource()
    {
    }
}
