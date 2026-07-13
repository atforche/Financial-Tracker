using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Value object class representing the balance of a Goal.
/// </summary>
public class GoalBalance
{
    /// <summary>
    /// Fund for this Goal Balance.
    /// </summary>
    public FundId FundId { get; }

    /// <summary>
    /// Amount assigned for this Goal Balance.
    /// </summary>
    public decimal AmountAssigned { get; }

    /// <summary>
    /// Pending amount assigned for this Goal Balance.
    /// </summary>
    public decimal PendingAmountAssigned { get; }

    /// <summary>
    /// Amount spent for this Goal Balance.
    /// </summary>
    public decimal AmountSpent { get; }

    /// <summary>
    /// Pending amount spent for this Goal Balance.
    /// </summary>
    public decimal PendingAmountSpent { get; }

    /// <summary>
    /// Adds the provided pending amount assigned to the current pending Goal Balance.
    /// </summary>
    internal GoalBalance AddNewPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, AmountAssigned, PendingAmountAssigned + pendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Posts the provided pending amount assigned to the current Goal Balance.
    /// </summary>
    internal GoalBalance PostPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, AmountAssigned + pendingAmountAssigned, PendingAmountAssigned - pendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Adds the provided pending amount spent to the current pending Goal Balance.
    /// </summary>
    internal GoalBalance AddNewPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent + pendingAmountSpent);

    /// <summary>
    /// Posts the provided pending amount spent to the current Goal Balance.
    /// </summary>
    internal GoalBalance PostPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent + pendingAmountSpent, PendingAmountSpent - pendingAmountSpent);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal GoalBalance(FundId fundId, decimal amountAssigned, decimal pendingAmountAssigned, decimal amountSpent, decimal pendingAmountSpent)
    {
        FundId = fundId;
        AmountAssigned = amountAssigned;
        PendingAmountAssigned = pendingAmountAssigned;
        AmountSpent = amountSpent;
        PendingAmountSpent = pendingAmountSpent;
    }
}