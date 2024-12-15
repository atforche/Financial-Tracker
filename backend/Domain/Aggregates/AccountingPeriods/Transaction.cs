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
    /// <param name="debitAccountInfo">Debit Account for this Transaction</param>
    /// <param name="creditAccountInfo">Credit Account for this Transaction</param>
    internal Transaction(AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        CreateBalanceEventAccountInfo? debitAccountInfo,
        CreateBalanceEventAccountInfo? creditAccountInfo,
        IEnumerable<FundAmount> accountingEntries)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        TransactionDate = transactionDate;
        _accountingEntries = accountingEntries.ToList();
        _transactionBalanceEvents = [];
        int nextEventSequence = accountingPeriod.GetNextEventSequenceForDate(transactionDate);
        if (debitAccountInfo != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                debitAccountInfo,
                TransactionDate,
                nextEventSequence++,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Debit));
        }
        if (creditAccountInfo != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                creditAccountInfo,
                TransactionDate,
                nextEventSequence++,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Credit));
        }
        ValidateNewTransaction();
    }

    /// <summary>
    /// Posts the Transaction in the provided Account
    /// </summary>
    /// <param name="accountInfo">Account that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    internal void Post(CreateBalanceEventAccountInfo accountInfo, DateOnly postedStatementDate)
    {
        ValidatePosting(accountInfo, postedStatementDate);
        _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
            accountInfo,
            postedStatementDate,
            AccountingPeriod.GetNextEventSequenceForDate(postedStatementDate),
            TransactionBalanceEventType.Posted,
            accountInfo.Account == GetAccount(TransactionAccountType.Debit)
                ? TransactionAccountType.Debit
                : TransactionAccountType.Credit));
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction()
        : base(new EntityId(default(long), Guid.NewGuid()))
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
    /// Validates a new Transaction
    /// </summary>
    private void ValidateNewTransaction()
    {
        if (TransactionDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        // Validate that an Accounting Entry was provided
        if (AccountingEntries.Count == 0)
        {
            throw new InvalidOperationException();
        }
        // Validate that no blank Accounting Entries were provided
        if (AccountingEntries.Any(entry => entry.Amount <= 0))
        {
            throw new InvalidOperationException();
        }
        // Validate that we don't have multiple Accounting Entries for the same Fund
        if (AccountingEntries.GroupBy(entry => entry.Fund.Id).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
        // Validate that either a debit or credit Account was provided
        if (TransactionBalanceEvents.Count == 0)
        {
            throw new InvalidOperationException();
        }
        // Validate that different Accounts must be provided for the debit and credit Accounts
        if (TransactionBalanceEvents
            .GroupBy(balanceEvent => balanceEvent.Account)
            .Any(balanceEvents => balanceEvents.DistinctBy(balanceEvents => balanceEvents.TransactionAccountType).Count() > 1))
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Validates posting a Transaction
    /// </summary>
    /// <param name="accountInfo">Account that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    private void ValidatePosting(CreateBalanceEventAccountInfo accountInfo, DateOnly postedStatementDate)
    {
        if (accountInfo.Account != GetAccount(TransactionAccountType.Debit) &&
            accountInfo.Account != GetAccount(TransactionAccountType.Credit))
        {
            throw new InvalidOperationException();
        }
        // Validate that this Transaction hasn't already been posted for the provided Account
        if (TransactionBalanceEvents.Any(balanceEvent => balanceEvent.Account == accountInfo.Account &&
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