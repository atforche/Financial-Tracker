using Domain.AccountingPeriods;
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
    /// Accounting Period that owns this posted movement.
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Cumulative posted change within this Accounting Period, excluding its opening balance.
    /// </summary>
    public decimal AccountingPeriodBalanceChange { get; private set; }

    /// <summary>
    /// Date for this Account Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Account Balance History
    /// </summary>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted Balance for this Account Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

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
    }

    /// <summary>
    /// Adjusts later within-period balances after an earlier movement changes.
    /// </summary>
    internal void AdjustAccountingPeriodBalanceChange(decimal change) => AccountingPeriodBalanceChange += change;

    /// <summary>
    /// Converts this Account Balance History to an Account Balance
    /// </summary>
    public AccountBalance ToAccountBalance() => new(Account, PostedBalance);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalanceHistory(Account account,
        TransactionId transactionId,
        AccountingPeriodId accountingPeriodId,
        DateOnly date,
        int sequence,
        AccountBalance accountBalance,
        decimal accountingPeriodBalanceChange)
        : base(new AccountBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        TransactionId = transactionId;
        AccountingPeriodId = accountingPeriodId;
        AccountingPeriodBalanceChange = accountingPeriodBalanceChange;
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
        AccountingPeriodId = null!;
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
