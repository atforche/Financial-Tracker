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
    /// Posted amount assigned toward the expected monthly contribution during
    /// the Accounting Period.
    /// </summary>
    public decimal AmountAssignedToExpectedContribution { get; }

    /// <summary>
    /// Amount remaining to assign toward the expected monthly contribution.
    /// </summary>
    public decimal RemainingAmountToAssignToExpectedContribution { get; }

    /// <summary>
    /// Posted amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; }

    /// <summary>
    /// Assigns the specified amount to this Fund Goal Totals.
    /// </summary>
    internal FundGoalTotals Assign(decimal amount, decimal? plannedAmount = null) => new(
        FundId,
        AmountAssigned + amount,
        AmountSpent,
        AmountAssignedToExpectedContribution + (plannedAmount ?? amount));

    /// <summary>
    /// Spends the specified amount from this Fund Goal Totals.
    /// </summary>
    internal FundGoalTotals Spend(decimal amount) => new(
        FundId,
        AmountAssigned,
        AmountSpent + amount,
        AmountAssignedToExpectedContribution);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundGoalTotals(
        FundId fundId,
        decimal amountAssigned,
        decimal amountSpent,
        decimal? amountAssignedToExpectedContribution = null,
        decimal? remainingAmountToAssignToExpectedContribution = null)
    {
        FundId = fundId;
        AmountAssigned = amountAssigned;
        AmountAssignedToExpectedContribution = amountAssignedToExpectedContribution ?? amountAssigned;
        AmountSpent = amountSpent;
        RemainingAmountToAssignToExpectedContribution = remainingAmountToAssignToExpectedContribution ?? 0;
    }

    /// <summary>
    /// Adds the remaining planned monthly contribution amount calculated for API projection.
    /// </summary>
    internal FundGoalTotals WithRemainingAmountToAssignToExpectedContribution(
        decimal remainingAmountToAssignToExpectedContribution) => new(
            FundId,
            AmountAssigned,
            AmountSpent,
            AmountAssignedToExpectedContribution,
            remainingAmountToAssignToExpectedContribution);
}
