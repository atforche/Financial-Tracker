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
    /// Amount assigned after current unposted Transaction effects are applied.
    /// </summary>
    public decimal AmountAssignedIncludingPending { get; }

    /// <summary>
    /// Amount assigned toward the expected monthly contribution after current
    /// unposted Transaction effects are applied.
    /// </summary>
    public decimal AmountAssignedToExpectedContributionIncludingPending { get; }

    /// <summary>
    /// Amount remaining to assign toward the expected monthly contribution.
    /// </summary>
    public decimal RemainingAmountToAssignToExpectedContribution { get; private set; }

    /// <summary>
    /// Amount remaining to assign toward the expected monthly contribution including pending effects.
    /// </summary>
    public decimal RemainingAmountToAssignToExpectedContributionIncludingPending { get; private set; }

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
        decimal? amountAssignedIncludingPending = null,
        decimal? amountAssignedToExpectedContributionIncludingPending = null,
        decimal? amountSpentIncludingPending = null,
        decimal? remainingAmountToAssignToExpectedContribution = null,
        decimal? remainingAmountToAssignToExpectedContributionIncludingPending = null)
    {
        FundId = fundId;
        AmountAssigned = amountAssigned;
        AmountAssignedIncludingPending = amountAssignedIncludingPending ?? amountAssigned;
        AmountAssignedToExpectedContribution = amountAssignedToExpectedContribution ?? amountAssigned;
        AmountAssignedToExpectedContributionIncludingPending = amountAssignedToExpectedContributionIncludingPending ?? AmountAssignedToExpectedContribution;
        AmountSpent = amountSpent;
        AmountSpentIncludingPending = amountSpentIncludingPending ?? amountSpent;
        RemainingAmountToAssignToExpectedContribution = remainingAmountToAssignToExpectedContribution ?? 0;
        RemainingAmountToAssignToExpectedContributionIncludingPending = remainingAmountToAssignToExpectedContributionIncludingPending ?? RemainingAmountToAssignToExpectedContribution;
    }

    /// <summary>
    /// Adds the remaining planned monthly contribution amounts calculated for API projection.
    /// </summary>
    internal FundGoalTotals WithRemainingAmountToAssignToExpectedContribution(
        decimal remainingAmountToAssignToExpectedContribution,
        decimal remainingAmountToAssignToExpectedContributionIncludingPending) => new(
            FundId,
            AmountAssigned,
            AmountSpent,
            AmountAssignedToExpectedContribution,
            AmountAssignedIncludingPending,
            AmountAssignedToExpectedContributionIncludingPending,
            AmountSpentIncludingPending,
            remainingAmountToAssignToExpectedContribution,
            remainingAmountToAssignToExpectedContributionIncludingPending);
}
