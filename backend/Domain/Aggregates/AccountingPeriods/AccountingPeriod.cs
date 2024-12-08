using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

/// <summary>
/// Entity class representing an Accounting Period
/// </summary>
/// <remarks>
/// An Accounting Period represents a month-long period used to organize transactions and track balances and budgets.
/// </remarks>
public class AccountingPeriod : EntityBase
{
    private readonly List<Transaction> _transactions = [];
    private readonly List<AccountBalanceCheckpoint> _accountBalanceCheckpoints = [];

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Gets the Period Start Date for this Accounting Period
    /// </summary>
    public DateOnly PeriodStartDate => new DateOnly(Year, Month, 1);

    /// <summary>
    /// Is Open flag for this Accounting Period
    /// </summary>
    /// <remarks>
    /// Once an Accounting Period has been closed, no changes can be made to anything thats fall within 
    /// that Accounting Period. Multiple Accounting Periods can be open at the same time, assuming all 
    /// the open periods represent a contiguous period of time. Only the earliest open period can be closed.
    /// </remarks>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Transactions for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    /// <summary>
    /// Account Balance Checkpoints for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<AccountBalanceCheckpoint> AccountBalanceCheckpoints => _accountBalanceCheckpoints;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    internal AccountingPeriod(int year, int month)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Year = year;
        Month = month;
        IsOpen = true;
        Validate();
    }

    /// <summary>
    /// Adds a new Transaction to this Accounting Period
    /// </summary>
    /// <param name="transactionDate">Transaction Date for this Transaction</param>
    /// <param name="debitAccount">Debit Account for this Transaction</param>
    /// <param name="creditAccount">Credit Account for this Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <param name="currentAccountBalances">Current Account Balances for the Accounts this Transaction affects</param>
    /// <param name="futureBalanceEventsForAccounts">All existing future balance events for the Accounts this Transaction affects</param>
    /// <returns>The newly created Transaction</returns>
    internal Transaction AddTransaction(DateOnly transactionDate,
        Account? debitAccount,
        Account? creditAccount,
        IEnumerable<FundAmount> accountingEntries,
        List<AccountBalanceByDate> currentAccountBalances,
        List<AccountBalanceByEvent> futureBalanceEventsForAccounts)
    {
        var newTransaction = new Transaction(this,
            transactionDate,
            accountingEntries,
            debitAccount,
            creditAccount,
            currentAccountBalances,
            futureBalanceEventsForAccounts);
        _transactions.Add(newTransaction);
        return newTransaction;
    }

    /// <summary>
    /// Gets the list of all Balance Events for this Accounting Period
    /// </summary>
    /// <returns>The list of all Balance Events for this Accounting Period</returns>
    internal IEnumerable<IBalanceEvent> GetAllBalanceEvents() =>
        Transactions.SelectMany(transaction => transaction.TransactionBalanceEvents)
            .OrderBy(balanceEvent => balanceEvent.EventDate)
            .ThenBy(balanceEvent => balanceEvent.EventSequence);

    /// <summary>
    /// Adds a new Account Balance Checkpoint to this Accounting Period
    /// </summary>
    /// <param name="account">Account for this Account Balance Checkpoint</param>
    /// <param name="fundBalances">Fund Balances for this Account Balance Checkpoint</param>
    internal void AddAccountBalanceCheckpoint(Account account, IEnumerable<FundAmount> fundBalances)
    {
        if (_accountBalanceCheckpoints.Any(checkpoint => checkpoint.Account == account))
        {
            throw new InvalidOperationException();
        }
        _accountBalanceCheckpoints.Add(new AccountBalanceCheckpoint(this, account, fundBalances));
    }

    /// <summary>
    /// Gets the next Balance Event sequence for the provided date
    /// </summary>
    /// <param name="eventDate">Event date to get the next Balance Event sequence for</param>
    /// <returns>The next Balance Event sequence for the provided date</returns>
    internal int GetNextEventSequenceForDate(DateOnly eventDate)
    {
        List<IBalanceEvent> balanceEventsOnDate = GetAllBalanceEvents()
            .Where(balanceEvent => balanceEvent.EventDate == eventDate).ToList();
        if (balanceEventsOnDate.Count == 0)
        {
            return 1;
        }
        return balanceEventsOnDate.Count + 1;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriod()
        : base(new EntityId(default(long), Guid.NewGuid()))
    {
    }

    /// <summary>
    /// Validates the current Accounting Period
    /// </summary>
    private void Validate()
    {
        if (Year < 2020 || Year > 2050)
        {
            throw new InvalidOperationException();
        }
        if (Month <= 0 || Month > 12)
        {
            throw new InvalidOperationException();
        }
    }
}