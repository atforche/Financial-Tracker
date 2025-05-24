using Domain.Accounts;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
/// <remarks>
/// A Transaction represents an event where money moves in one of the following ways:
/// 1. Money is debited from an Account
/// 2. Money is credited to an Account
/// 3. Money is debited from one Account and credited to another Account
/// If money moves from one Account to another, the amount debited is equal to the amount credited. 
/// </remarks>
public class Transaction : Entity
{
    private readonly List<FundAmount> _accountingEntries;
    private readonly List<TransactionBalanceEvent> _transactionBalanceEvents;

    /// <summary>
    /// Accounting Period for this Transaction
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// List of Accounting Entries for this Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> AccountingEntries => _accountingEntries;

    /// <summary>
    /// List of Balance Events for this Transaction
    /// </summary>
    public IReadOnlyCollection<TransactionBalanceEvent> TransactionBalanceEvents => _transactionBalanceEvents;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Parent Accounting Period for this Transaction</param>
    /// <param name="eventDate">Date for this Transaction</param>
    /// <param name="eventSequence">Event Sequence for the initial Transaction Balance Event</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <param name="debitAccount">Debit Account for this Transaction</param>
    /// <param name="creditAccount">Credit Account for this Transaction</param>
    internal Transaction(AccountingPeriod accountingPeriod,
        DateOnly eventDate,
        int eventSequence,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries)
    {
        AccountingPeriod = accountingPeriod;
        Date = eventDate;
        _accountingEntries = accountingEntries.ToList();
        _transactionBalanceEvents = [];
        if (debitAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                Date,
                eventSequence,
                debitAccount,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Debit));
        }
        if (creditAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                Date,
                debitAccount != null ? eventSequence + 1 : eventSequence,
                creditAccount,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Credit));
        }
    }

    /// <summary>
    /// Gets the Account for this Transaction that corresponds to the provided Transaction Account Type
    /// </summary>
    /// <param name="accountType">Type of the Account to get</param>
    /// <returns>The Account for this Transaction that corresponds to the provided type</returns>
    internal Account? GetAccount(TransactionAccountType accountType) =>
        TransactionBalanceEvents.SingleOrDefault(balanceEvent =>
            balanceEvent.AccountType == accountType &&
            balanceEvent.EventType == TransactionBalanceEventType.Added)?.Account;

    /// <summary>
    /// Posts the Transaction in the provided Account
    /// </summary>
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    /// <param name="eventSequence">Event Sequence for this Transaction Balance Event</param>
    internal void Post(TransactionAccountType accountType, DateOnly postedStatementDate, int eventSequence) =>
        _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
            postedStatementDate,
            eventSequence,
            GetAccount(accountType) ?? throw new InvalidOperationException(),
            TransactionBalanceEventType.Posted,
            accountType));

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction()
    {
        AccountingPeriod = null!;
        _accountingEntries = [];
        _transactionBalanceEvents = [];
    }
}