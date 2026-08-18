using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;

namespace Domain.Transactions.Spending;

/// <summary>
/// Value object representing the destination of money for a spending transaction.
/// </summary>
public class SpendingTransactionDestination
{
    private readonly List<FundAmount> _fundAssignments = [];

    /// <summary>
    /// Account of the spending transaction destination.
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted Date of the spending transaction destination.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the spending transaction destination.
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Amount of the spending transaction destination.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Fund assignments of the spending transaction destination.
    /// </summary>
    public IReadOnlyList<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public SpendingTransactionDestination(Account? account, DateOnly? postedDate, Location? location, decimal amount, List<FundAmount> fundAssignments)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
        Amount = amount;
        _fundAssignments.AddRange(fundAssignments);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private SpendingTransactionDestination()
    {
    }
}
