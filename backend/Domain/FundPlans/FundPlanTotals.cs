using Domain.Funds;

namespace Domain.FundPlans;

/// <summary>
/// Value object representing assignment and spending totals for a Fund Plan.
/// </summary>
public sealed class FundPlanTotals
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
    /// Pending amount assigned during the Accounting Period.
    /// </summary>
    public decimal PendingAmountAssigned { get; }

    /// <summary>
    /// Posted amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; }

    /// <summary>
    /// Pending amount spent during the Accounting Period.
    /// </summary>
    public decimal PendingAmountSpent { get; }

    /// <summary>
    /// Adds a new pending amount assigned to the Fund Plan totals.
    /// </summary>
    internal FundPlanTotals AddNewPendingAmountAssigned(decimal amount) =>
        new(FundId, AmountAssigned, PendingAmountAssigned + amount, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Posts a pending amount assigned to the Fund Plan totals.
    /// </summary>
    internal FundPlanTotals PostPendingAmountAssigned(decimal amount) =>
        new(FundId, AmountAssigned + amount, PendingAmountAssigned - amount, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Adds a new pending amount spent to the Fund Plan totals.
    /// </summary>
    internal FundPlanTotals AddNewPendingAmountSpent(decimal amount) =>
        new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent + amount);

    /// <summary>
    /// Posts a pending amount spent to the Fund Plan totals.
    /// </summary>
    internal FundPlanTotals PostPendingAmountSpent(decimal amount) =>
        new(FundId, AmountAssigned, PendingAmountAssigned, AmountSpent + amount, PendingAmountSpent - amount);

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal FundPlanTotals(
        FundId fundId,
        decimal amountAssigned,
        decimal pendingAmountAssigned,
        decimal amountSpent,
        decimal pendingAmountSpent)
    {
        FundId = fundId;
        AmountAssigned = amountAssigned;
        PendingAmountAssigned = pendingAmountAssigned;
        AmountSpent = amountSpent;
        PendingAmountSpent = pendingAmountSpent;
    }
}