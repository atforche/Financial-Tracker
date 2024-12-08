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
        // Validate that this posting date falls after the Transaction was added
        if (TransactionBalanceEvents.Any(balanceEvent =>
                balanceEvent.TransactionEventType == TransactionBalanceEventType.Added &&
                balanceEvent.EventDate > postedStatementDate))
        {
            throw new InvalidOperationException();
        }
        // Validate that a transaction can only be posted with a date in a month adjacent to the Accounting Period month
        int monthDifference = (AccountingPeriod.Year - postedStatementDate.Year) * 12 + AccountingPeriod.Month - postedStatementDate.Month;
        if (Math.Abs(monthDifference) > 1)
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
    /// <param name="currentAccountBalances">Current Account Balances for the Accounts this Transaction affects</param>
    /// <param name="futureBalanceEventsForAccounts">All existing future balance events for the Accounts this Transaction affects</param>
    internal Transaction(AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        IEnumerable<FundAmount> accountingEntries,
        Account? debitAccount,
        Account? creditAccount,
        List<AccountBalanceByDate> currentAccountBalances,
        List<AccountBalanceByEvent> futureBalanceEventsForAccounts)
        : base(new EntityId(default, Guid.NewGuid()))
    {
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
        Validate(currentAccountBalances, futureBalanceEventsForAccounts);
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
    /// <param name="currentAccountBalances">Current Account Balances for the Accounts this Transaction affects</param>
    /// <param name="futureBalanceEventsForAccounts">All existing future balance events for the Accounts this Transaction affects</param>
    private void Validate(
        List<AccountBalanceByDate> currentAccountBalances,
        List<AccountBalanceByEvent> futureBalanceEventsForAccounts)
    {
        if (TransactionDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        // Validate that a transaction can only be added with a date in a month adjacent to the Accounting Period month
        int monthDifference = (AccountingPeriod.Year - TransactionDate.Year) * 12 + AccountingPeriod.Month - TransactionDate.Month;
        if (Math.Abs(monthDifference) > 1)
        {
            throw new InvalidOperationException();
        }
        if (!AccountingPeriod.IsOpen)
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
        // Validate that adding this Transaction doesn't cause the current balances of any Accounts to
        // go into the negative
        foreach (AccountBalanceByDate accountBalanceByDate in currentAccountBalances)
        {
            if (TransactionBalanceEvents.Any(balanceEvent =>
                    balanceEvent.ApplyEventToBalance(accountBalanceByDate.AccountBalance).BalanceIncludingPending < 0))
            {
                throw new InvalidOperationException();
            }
        }
        // Validate that adding this Transaction doesn't cause any of the existing balance events in the future to 
        // push an Account into the negative
        foreach (AccountBalanceByEvent accountBalanceByEvent in futureBalanceEventsForAccounts)
        {
            if (TransactionBalanceEvents.Any(balanceEvent =>
                    balanceEvent.ApplyEventToBalance(accountBalanceByEvent.AccountBalance).BalanceIncludingPending < 0))
            {
                throw new InvalidOperationException();
            }
        }
        // Validate that adding this Transaction doesn't cause any of the existing balance events in this Accounting Period
        // in the future to push an Account into the negative
        foreach (AccountBalanceByEvent accountBalanceByEvent in futureBalanceEventsForAccounts
                    .Where(balanceEvent => balanceEvent.BalanceEvent.AccountingPeriod == AccountingPeriod))
        {
            if (TransactionBalanceEvents.Any(balanceEvent =>
                    balanceEvent.ApplyEventToBalance(accountBalanceByEvent.AccountBalance).BalanceIncludingPending < 0))
            {
                throw new InvalidOperationException();
            }
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
}