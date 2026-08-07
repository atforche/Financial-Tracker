using Domain.Funds;

namespace Domain.FundGoals;

/// <summary>
/// Value object representing assignment and spending totals for a Fund Goal.
/// </summary>
public sealed class FundGoalTotals
{
    /// <summary>
    /// Fund associated with this totals.
    /// </summary>
    public FundId FundId { get; }

    /// <summary>
    /// Posted amount assigned during the Accounting Period.
    /// </summary>
    public decimal AmountAssigned { get; }

    /// <summary>
    /// Posted amount assigned toward the regular monthly contribution during
    /// the Accounting Period.
    /// </summary>
    public decimal RegularAmountAssigned { get; }

    /// <summary>
    /// Amount assigned after current unposted Transaction effects are applied.
    /// </summary>
    public decimal AmountAssignedIncludingPending { get; }

    /// <summary>
    /// Amount assigned toward the regular monthly contribution after current
    /// unposted Transaction effects are applied.
    /// </summary>
    public decimal RegularAmountAssignedIncludingPending { get; }

    /// <summary>
    /// Posted amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; }

    /// <summary>
    /// Amount spent after current unposted Transaction effects are applied.
    /// </summary>
    public decimal AmountSpentIncludingPending { get; }

    /// <summary>
    /// Assigns the specified amount to this Fund Goal Totals.
    /// </summary>
    internal FundGoalTotals Assign(decimal amount, decimal? regularAmount = null) => new(
        FundId,
        AmountAssigned + amount,
        AmountSpent,
        RegularAmountAssigned + (regularAmount ?? amount));

    /// <summary>
    /// Spends the specified amount from this Fund Goal Totals.
    /// </summary>
    internal FundGoalTotals Spend(decimal amount) => new(
        FundId,
        AmountAssigned,
        AmountSpent + amount,
        RegularAmountAssigned);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalTotals(
        FundId fundId,
        decimal amountAssigned,
        decimal amountSpent,
        decimal? regularAmountAssigned = null,
        decimal? amountAssignedIncludingPending = null,
        decimal? regularAmountAssignedIncludingPending = null,
        decimal? amountSpentIncludingPending = null)
    {
        FundId = fundId;
        AmountAssigned = amountAssigned;
        AmountAssignedIncludingPending = amountAssignedIncludingPending ?? amountAssigned;
        RegularAmountAssigned = regularAmountAssigned ?? amountAssigned;
        RegularAmountAssignedIncludingPending = regularAmountAssignedIncludingPending ?? RegularAmountAssigned;
        AmountSpent = amountSpent;
        AmountSpentIncludingPending = amountSpentIncludingPending ?? amountSpent;
    }
}