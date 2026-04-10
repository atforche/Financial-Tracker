namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Account for this Account Balance
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Posted Balance for this Account Balance
    /// </summary>
    public decimal PostedBalance { get; }

    /// <summary>
    /// Pending Debit Amount for this Account Balance
    /// </summary>
    public decimal PendingDebitAmount { get; }

    /// <summary>
    /// Pending Credit Amount for this Account Balance
    /// </summary>
    public decimal PendingCreditAmount { get; }

    /// <summary>
    /// Available to Spend Balance for this Account Balance
    /// </summary>
    public decimal? AvailableToSpend => Account.Type == AccountType.Debt ? null : PostedBalance - PendingDebitAmount;

    /// <summary>
    /// Adds the provided pending debit amount to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingDebitAmount(decimal pendingDebitAmount) =>
        new(Account, PostedBalance, PendingDebitAmount + pendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Posts the provided pending debit amount to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingDebitAmount(decimal pendingDebitAmount) =>
        new(Account, PostedBalance - pendingDebitAmount, PendingDebitAmount - pendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Adds the provided pending credit amount to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingCreditAmount(decimal pendingCreditAmount) =>
        new(Account, PostedBalance, PendingDebitAmount, PendingCreditAmount + pendingCreditAmount);

    /// <summary>
    /// Posts the provided pending credit amount to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingCreditAmount(decimal pendingCreditAmount) =>
        new(Account, PostedBalance + pendingCreditAmount, PendingDebitAmount, PendingCreditAmount - pendingCreditAmount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(Account account, decimal postedBalance, decimal pendingDebitAmount, decimal pendingCreditAmount)
    {
        Account = account;
        PostedBalance = postedBalance;
        PendingDebitAmount = pendingDebitAmount;
        PendingCreditAmount = pendingCreditAmount;
    }
}