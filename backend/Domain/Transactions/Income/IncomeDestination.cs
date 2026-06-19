using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Individual income destination for an income transaction.
/// </summary>
public class IncomeDestination
{
    private readonly List<FundAmount> _fundAssignments = [];

    /// <summary>
    /// Account for this income destination.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Amount for this income destination.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Posted Date for this income destination.
    /// </summary>
    public DateOnly? PostedDate { get; private set; }

    /// <summary>
    /// Fund assignments for this income destination.
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public IncomeDestination(Account account, decimal amount, DateOnly? postedDate, IEnumerable<FundAmount> fundAssignments)
    {
        Account = account;
        Amount = amount;
        PostedDate = postedDate;
        _fundAssignments.AddRange(fundAssignments);
        if (Amount != _fundAssignments.Sum(f => f.Amount))
        {
            _fundAssignments.Add(new FundAmount
            {
                FundId = Fund.UnassignedFundId,
                Amount = Amount - _fundAssignments.Sum(f => f.Amount)
            });
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeDestination()
    {
        Account = null!;
    }
}
