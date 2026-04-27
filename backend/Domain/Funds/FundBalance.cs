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
    /// Amount assigned for this Fund Balance
    /// </summary>
    public decimal AmountAssigned { get; }

    /// <summary>
    /// Pending amount assigned for this Fund Balance
    /// </summary>
    public decimal PendingAmountAssigned { get; }

    /// <summary>
    /// Amount spent for this Fund Balance
    /// </summary>
    public decimal AmountSpent { get; }

    /// <summary>
    /// Pending amount spent for this Fund Balance
    /// </summary>
    public decimal PendingAmountSpent { get; }

    /// <summary>
    /// Adds the provided pending amount assigned to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, PostedBalance, AmountAssigned, PendingAmountAssigned + pendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Posts the provided pending amount assigned to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingAmountAssigned(decimal pendingAmountAssigned) =>
        new(FundId, PostedBalance + pendingAmountAssigned, AmountAssigned, PendingAmountAssigned - pendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Adds the provided pending amount spent to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, PostedBalance, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent + pendingAmountSpent);

    /// <summary>
    /// Posts the provided pending amount spent to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingAmountSpent(decimal pendingAmountSpent) =>
        new(FundId, PostedBalance - pendingAmountSpent, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent - pendingAmountSpent);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(
        FundId fundId,
        decimal postedBalance,
        decimal amountAssigned,
        decimal pendingAmountAssigned,
        decimal amountSpent,
        decimal pendingAmountSpent)
    {
        FundId = fundId;
        PostedBalance = postedBalance;
        AmountAssigned = amountAssigned;
        PendingAmountAssigned = pendingAmountAssigned;
        AmountSpent = amountSpent;
        PendingAmountSpent = pendingAmountSpent;
    }
}