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
    /// Balance including pending debit and credit amounts for this Account Balance
    /// </summary>
    public decimal PendingBalance => CalculatePendingBalance(
        Account.Type,
        PostedBalance,
        PendingDebitAmount,
        PendingCreditAmount);

    /// <summary>
    /// Calculates the balance including pending amounts for the provided Account Type
    /// </summary>
    internal static decimal CalculatePendingBalance(
        AccountType accountType,
        decimal postedBalance,
        decimal pendingDebitAmount,
        decimal pendingCreditAmount) => accountType.IsDebt()
            ? postedBalance + pendingDebitAmount - pendingCreditAmount
            : postedBalance - pendingDebitAmount + pendingCreditAmount;

    /// <summary>
    /// Adds the provided pending debit amount to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingDebitAmount(decimal pendingDebitAmount) =>
        new(Account, PostedBalance, PendingDebitAmount + pendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Posts the provided pending debit amount to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingDebitAmount(decimal pendingDebitAmount) => new(
        Account,
        PostedBalance + (Account.Type.IsDebt() ? pendingDebitAmount : -pendingDebitAmount),
        PendingDebitAmount - pendingDebitAmount,
        PendingCreditAmount);

    /// <summary>
    /// Adds the provided pending credit amount to the current pending Account Balance
    /// </summary>
    internal AccountBalance AddNewPendingCreditAmount(decimal pendingCreditAmount) =>
        new(Account, PostedBalance, PendingDebitAmount, PendingCreditAmount + pendingCreditAmount);

    /// <summary>
    /// Posts the provided pending credit amount to the current Account Balance
    /// </summary>
    internal AccountBalance PostPendingCreditAmount(decimal pendingCreditAmount) => new(
        Account,
        PostedBalance + (Account.Type.IsDebt() ? -pendingCreditAmount : pendingCreditAmount),
        PendingDebitAmount,
        PendingCreditAmount - pendingCreditAmount);

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