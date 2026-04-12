namespace Domain.Funds;

/// <summary>
/// Value object class representing the balance of a Fund
/// </summary>
public class FundBalance
{
    /// <summary>
    /// Fund for this Fund Balance
    /// </summary>
    public FundId FundId { get; }

    /// <summary>
    /// Posted Balance for this Fund Balance
    /// </summary>
    public decimal PostedBalance { get; }

    /// <summary>
    /// Pending amount spent for this Fund Balance
    /// </summary>
    public decimal PendingAmountSpent { get; }

    /// <summary>
    /// Pending amount assigned for this Fund Balance
    /// </summary>
    public decimal PendingAmountAssigned { get; }

    /// <summary>
    /// Adds the provided pending amount spent to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, PostedBalance, PendingAmountSpent + pendingAmountSpent, PendingAmountAssigned);

    /// <summary>
    /// Posts the provided pending amount spent to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, PostedBalance - pendingAmountSpent, PendingAmountSpent - pendingAmountSpent, PendingAmountAssigned);

    /// <summary>
    /// Adds the provided pending amount assigned to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, PostedBalance, PendingAmountSpent, PendingAmountAssigned + pendingAmountAssigned);

    /// <summary>
    /// Posts the provided pending amount assigned to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, PostedBalance + pendingAmountAssigned, PendingAmountSpent, PendingAmountAssigned - pendingAmountAssigned);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(FundId fundId, decimal postedBalance, decimal pendingAmountSpent, decimal pendingAmountAssigned)
    {
        FundId = fundId;
        PostedBalance = postedBalance;
        PendingAmountSpent = pendingAmountSpent;
        PendingAmountAssigned = pendingAmountAssigned;
    }
}