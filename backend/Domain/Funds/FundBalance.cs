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
    /// Adds the provided pending debit amount to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingDebitAmount(decimal pendingDebitAmount) =>
        new(FundId, PostedBalance, PendingDebitAmount + pendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Posts the provided pending debit amount to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingDebit(decimal pendingDebitAmount) =>
        new(FundId, PostedBalance - pendingDebitAmount, PendingDebitAmount - pendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Adds the provided pending credit amount to the current pending Fund Balance
    /// </summary>
    internal FundBalance AddNewPendingCreditAmount(decimal pendingCreditAmount) =>
        new(FundId, PostedBalance, PendingDebitAmount, PendingCreditAmount + pendingCreditAmount);

    /// <summary>
    /// Posts the provided pending credit amount to the current posted Fund Balance
    /// </summary>
    internal FundBalance PostPendingCredit(decimal pendingCreditAmount) =>
        new(FundId, PostedBalance + pendingCreditAmount, PendingDebitAmount, PendingCreditAmount - pendingCreditAmount);

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