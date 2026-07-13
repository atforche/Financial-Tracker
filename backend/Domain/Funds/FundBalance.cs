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
    /// Pending Debit Amount for this Fund Balance
    /// </summary>
    public decimal PendingDebitAmount { get; }

    /// <summary>
    /// Pending Credit Amount for this Fund Balance
    /// </summary>
    public decimal PendingCreditAmount { get; }

    /// <summary>
    /// Adds a new pending debit amount to this Fund Balance.
    /// </summary>
    internal FundBalance AddNewPendingDebitAmount(decimal amount) => new(FundId, PostedBalance, PendingDebitAmount + amount, PendingCreditAmount);

    /// <summary>
    /// Posts a pending debit amount to this Fund Balance.
    /// </summary>
    internal FundBalance PostPendingDebitAmount(decimal amount) => new(FundId, PostedBalance - amount, PendingDebitAmount - amount, PendingCreditAmount);

    /// <summary>
    /// Adds a new pending credit amount to this Fund Balance.
    /// </summary>
    internal FundBalance AddNewPendingCreditAmount(decimal amount) => new(FundId, PostedBalance, PendingDebitAmount, PendingCreditAmount + amount);

    /// <summary>
    /// Posts a pending credit amount to this Fund Balance.
    /// </summary>
    internal FundBalance PostPendingCreditAmount(decimal amount) => new(FundId, PostedBalance + amount, PendingDebitAmount, PendingCreditAmount - amount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalance(FundId fundId, decimal postedBalance, decimal pendingDebitAmount, decimal pendingCreditAmount)
    {
        FundId = fundId;
        PostedBalance = postedBalance;
        PendingDebitAmount = pendingDebitAmount;
        PendingCreditAmount = pendingCreditAmount;
    }
}