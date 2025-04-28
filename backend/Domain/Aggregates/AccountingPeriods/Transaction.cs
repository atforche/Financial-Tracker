using Domain.Actions;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

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
public class Transaction : EntityBase
{
    private readonly List<FundAmount> _accountingEntries;
    private readonly List<TransactionBalanceEvent> _transactionBalanceEvents;

    /// <summary>
    /// Accounting Period for this Transaction
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; private set; }

    /// <summary>
    /// Transaction Date for this Transaction
    /// </summary>
    public DateOnly TransactionDate { get; private set; }

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
    /// <param name="transactionDate">Transaction Date for this Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <param name="debitAccount">Debit Account for this Transaction</param>
    /// <param name="creditAccount">Credit Account for this Transaction</param>
    internal Transaction(AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        TransactionDate = transactionDate;
        _accountingEntries = accountingEntries.ToList();
        _transactionBalanceEvents = [];
        if (debitAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                debitAccount,
                TransactionDate,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Debit));
        }
        if (creditAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                creditAccount,
                TransactionDate,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Credit,
                debitAccount != null ? 1 : 0));
        }
    }

    /// <summary>
    /// Posts the Transaction in the provided Account
    /// </summary>
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    public void Post(TransactionAccountType accountType, DateOnly postedStatementDate)
    {
        ValidatePosting(accountType, postedStatementDate);
        _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
            GetAccount(accountType) ?? throw new InvalidOperationException(),
            postedStatementDate,
            TransactionBalanceEventType.Posted,
            accountType));
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction()
        : base(new EntityId(default, Guid.NewGuid()))
    {
        AccountingPeriod = null!;
        _accountingEntries = [];
        _transactionBalanceEvents = [];
    }

    /// <summary>
    /// Gets the Account for this Transaction that corresponds to the provided Transaction Account Type
    /// </summary>
    /// <param name="accountType">Type of the Account to get</param>
    /// <returns>The Account for this Transaction that corresponds to the provided type</returns>
    private Account? GetAccount(TransactionAccountType accountType) =>
        TransactionBalanceEvents.SingleOrDefault(balanceEvent =>
            balanceEvent.TransactionAccountType == accountType &&
            balanceEvent.TransactionEventType == TransactionBalanceEventType.Added)?.Account;

    /// <summary>
    /// Validates posting a Transaction
    /// </summary>
    /// <param name="accountType">Account Type that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    private void ValidatePosting(TransactionAccountType accountType, DateOnly postedStatementDate)
    {
        if (GetAccount(accountType) == null)
        {
            throw new InvalidOperationException();
        }
        if (!AddBalanceEventAction.IsValid(AccountingPeriod, postedStatementDate, out Exception? exception))
        {
            throw exception;
        }
        // Validate that this Transaction hasn't already been posted for the provided Account
        if (TransactionBalanceEvents.Any(balanceEvent => balanceEvent.TransactionAccountType == accountType &&
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Posted))
        {
            throw new InvalidOperationException();
        }
        // Validate that this posting date falls after the Transaction was added
        if (TransactionBalanceEvents.Any(balanceEvent =>
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Added &&
                balanceEvent.EventDate > postedStatementDate))
        {
            throw new InvalidOperationException();
        }
    }
}