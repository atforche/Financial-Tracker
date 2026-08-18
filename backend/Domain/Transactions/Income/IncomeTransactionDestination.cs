using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Value object representing the destination of money for an income transaction.
/// </summary>
public class IncomeTransactionDestination
{
    private readonly List<IncomeFundAmount> _fundAssignments = [];

    /// <summary>
    /// Account for this income transaction destination.
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Posted Date for this income transaction destination.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Amount for this income transaction destination.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Fund assignments for this income transaction destination.
    /// </summary>
    public IReadOnlyCollection<IncomeFundAmount> FundAssignments => _fundAssignments;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public IncomeTransactionDestination(Account account, decimal amount, DateOnly? postedDate, IEnumerable<IncomeFundAmount> fundAssignments)
    {
        Account = account;
        Amount = amount;
        PostedDate = postedDate;
        _fundAssignments.AddRange(fundAssignments);
        if (Account.Type.IsTracked() && Amount != _fundAssignments.Sum(f => f.Amount))
        {
            _fundAssignments.Add(new IncomeFundAmount
            {
                FundId = Fund.UnassignedFundId,
                Amount = Amount - _fundAssignments.Sum(f => f.Amount)
            });
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeTransactionDestination()
    {
        Account = null!;
    }
}
