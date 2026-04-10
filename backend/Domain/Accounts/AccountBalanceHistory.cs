using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing the balance of an Account at some point in time.
/// </summary>
public class AccountBalanceHistory : Entity<AccountBalanceHistoryId>
{
    /// <summary>
    /// Account for this Account Balance History
    /// </summary>
    public Account Account { get; init; }

    /// <summary>
    /// Transaction ID for this Account Balance History
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date for this Account Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Account Balance History
    /// </summary>
    /// <remarks>
    /// The sequence number is used to order multiple balance history records for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted Balance for this Account Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

    /// <summary>
    /// Pending Debit Amount for this Account Balance History
    /// </summary>
    public decimal PendingDebitAmount { get; private set; }

    /// <summary>
    /// Pending Credit Amount for this Account Balance History
    /// </summary>
    public decimal PendingCreditAmount { get; private set; }

    /// <summary>
    /// Available to Spend for this Account Balance History
    /// </summary>
    public decimal? AvailableToSpend { get; private set; }

    /// <summary>
    /// Updates this Account Balance History with a new Account Balance.
    /// </summary>
    public void Update(AccountBalance newBalance)
    {
        if (newBalance.Account.Id != Account.Id)
        {
            throw new ArgumentException("New balance must be for the same account", nameof(newBalance));
        }
        PostedBalance = newBalance.PostedBalance;
        PendingDebitAmount = newBalance.PendingDebitAmount;
        PendingCreditAmount = newBalance.PendingCreditAmount;
        AvailableToSpend = newBalance.AvailableToSpend;
    }

    /// <summary>
    /// Converts this Account Balance History to an Account Balance
    /// </summary>
    /// <returns></returns>
    public AccountBalance ToAccountBalance() => new(Account, PostedBalance, PendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalanceHistory(Account account,
        TransactionId transactionId,
        DateOnly date,
        int sequence,
        AccountBalance accountBalance)
        : base(new AccountBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        Update(accountBalance);
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private AccountBalanceHistory()
    {
        Account = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountBalanceHistory"/>
/// </summary>
public record AccountBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal AccountBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}