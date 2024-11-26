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
    /// Posts the Transaction in the provided Account
    /// </summary>
    /// <param name="account">Account that this Transaction should be posted in</param>
    /// <param name="postedStatementDate">Posted statement date for this Transaction in the provided Account</param>
    public void Post(Account account, DateOnly postedStatementDate)
    {
        // Validate that the provided account is either the credit or debit Account for this Transaction
        Account? debitAccount = TransactionBalanceEvents
            .SingleOrDefault(balanceEvent => balanceEvent.TransactionAccountType == TransactionAccountType.Debit &&
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Added)?.Account;
        Account? creditAccount = TransactionBalanceEvents
            .SingleOrDefault(balanceEvent => balanceEvent.TransactionAccountType == TransactionAccountType.Credit &&
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Added)?.Account;
        if (account != debitAccount && account != creditAccount)
        {
            throw new InvalidOperationException();
        }
        // Validate that this Transaction hasn't already been posted for the provided Account
        if (TransactionBalanceEvents.Any(balanceEvent => balanceEvent.Account == account &&
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Posted))
        {
            throw new InvalidOperationException();
        }
        _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
            account,
            postedStatementDate,
            AccountingPeriod.GetNextEventSequenceForDate(postedStatementDate),
            TransactionBalanceEventType.Posted,
            account == debitAccount ? TransactionAccountType.Debit : TransactionAccountType.Credit));
    }

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
        IEnumerable<FundAmount> accountingEntries,
        Account? debitAccount,
        Account? creditAccount)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        if (debitAccount == null && creditAccount == null)
        {
            throw new InvalidOperationException();
        }

        AccountingPeriod = accountingPeriod;
        TransactionDate = transactionDate;
        _accountingEntries = accountingEntries.ToList();
        _transactionBalanceEvents = [];
        int nextEventSequence = accountingPeriod.GetNextEventSequenceForDate(transactionDate);
        if (debitAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                debitAccount,
                TransactionDate,
                nextEventSequence++,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Debit));
        }
        if (creditAccount != null)
        {
            _transactionBalanceEvents.Add(new TransactionBalanceEvent(this,
                creditAccount,
                TransactionDate,
                nextEventSequence++,
                TransactionBalanceEventType.Added,
                TransactionAccountType.Credit));
        }
        Validate();
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
    /// Validates the current Transaction
    /// </summary>
    private void Validate()
    {
        if (TransactionDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        if (AccountingEntries.Count == 0 ||
            AccountingEntries.Sum(entry => entry.Amount) == 0 ||
            AccountingEntries.GroupBy(entry => entry.Fund.Id).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
    }
}