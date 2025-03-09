using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
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
    private readonly List<FundConversion> _fundConversions = [];
    private readonly List<ChangeInValue> _changeInValues = [];
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
    /// Fund Conversions for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<FundConversion> FundConversions => _fundConversions;

    /// <summary>
    /// Change In Values for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<ChangeInValue> ChangeInValues => _changeInValues;

    /// <summary>
    /// Account Balance Checkpoints for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<AccountBalanceCheckpoint> AccountBalanceCheckpoints => _accountBalanceCheckpoints;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    /// <param name="existingAccountingPeriodStartDates">Period Start Dates for any existing Accounting Periods</param>
    internal AccountingPeriod(int year, int month, IEnumerable<DateOnly> existingAccountingPeriodStartDates)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Year = year;
        Month = month;
        IsOpen = true;
        ValidateNewAccountingPeriod(existingAccountingPeriodStartDates.ToList());
    }

    /// <summary>
    /// Adds a new Transaction to this Accounting Period
    /// </summary>
    /// <param name="transactionDate">Transaction Date for this Transaction</param>
    /// <param name="debitAccountInfo">Debit Account for this Transaction</param>
    /// <param name="creditAccountInfo">Credit Account for this Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <returns>The newly created Transaction</returns>
    internal Transaction AddTransaction(DateOnly transactionDate,
        CreateBalanceEventAccountInfo? debitAccountInfo,
        CreateBalanceEventAccountInfo? creditAccountInfo,
        IEnumerable<FundAmount> accountingEntries)
    {
        var newTransaction = new Transaction(this,
            transactionDate,
            debitAccountInfo,
            creditAccountInfo,
            accountingEntries);
        _transactions.Add(newTransaction);
        return newTransaction;
    }

    /// <summary>
    /// Adds a new Fund Conversion to this Accounting Period
    /// </summary>
    /// <param name="date">Date for this Fund Conversion</param>
    /// <param name="accountInfo">Account for this Fund Conversion</param>
    /// <param name="fromFund">From Fund for this Fund Conversion</param>
    /// <param name="toFund">To Fund for this Fund Conversion</param>
    /// <param name="amount">Amount for this Fund Conversion</param>
    /// <returns>The newly created Fund Conversion</returns>
    internal FundConversion AddFundConversion(DateOnly date,
        CreateBalanceEventAccountInfo accountInfo,
        Fund fromFund,
        Fund toFund,
        decimal amount)
    {
        var newFundConversion = new FundConversion(this,
            accountInfo,
            date,
            fromFund,
            toFund,
            amount);
        _fundConversions.Add(newFundConversion);
        return newFundConversion;
    }

    /// <summary>
    /// Adds a new Change In Value to this Accounting Period
    /// </summary>
    /// <param name="date">Date for this Change In Value</param>
    /// <param name="accountInfo">Account for this Change In Value</param>
    /// <param name="accountingEntry">Accounting Entry for this Change In Value</param>
    /// <returns>The newly created Change In Value</returns>
    internal ChangeInValue AddChangeInValue(DateOnly date,
        CreateBalanceEventAccountInfo accountInfo,
        FundAmount accountingEntry)
    {
        var newChangeInValue = new ChangeInValue(this,
            accountInfo,
            date,
            accountingEntry);
        _changeInValues.Add(newChangeInValue);
        return newChangeInValue;
    }

    /// <summary>
    /// Closes this Accounting Period
    /// </summary>
    /// <param name="otherOpenAccountingPeriods">List of the Period Start Dates for any other open Accounting Periods</param>
    /// <param name="allAccountBalances">Ending Account Balances for every Account at the end of this Accounting Period</param>
    internal void ClosePeriod(
        IEnumerable<DateOnly> otherOpenAccountingPeriods,
        IEnumerable<AccountBalanceByAccountingPeriod> allAccountBalances)
    {
        ValidateCloseAccountingPeriod(otherOpenAccountingPeriods, allAccountBalances);
        IsOpen = false;
    }

    /// <summary>
    /// Gets the list of all Balance Events for this Accounting Period
    /// </summary>
    /// <returns>The list of all Balance Events for this Accounting Period</returns>
    internal IEnumerable<BalanceEventBase> GetAllBalanceEvents() =>
        Transactions.SelectMany(transaction => (IEnumerable<BalanceEventBase>)transaction.TransactionBalanceEvents)
            .Concat(FundConversions)
            .Concat(ChangeInValues)
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
        var balanceEventsOnDate = GetAllBalanceEvents().Where(balanceEvent => balanceEvent.EventDate == eventDate).ToList();
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
        : base(new EntityId(default, Guid.NewGuid()))
    {
    }

    /// <summary>
    /// Validates a new Accounting Period
    /// </summary>
    /// <param name="existingAccountingPeriodStartDates">Period Start Dates for any existing Accounting Periods</param>
    private void ValidateNewAccountingPeriod(List<DateOnly> existingAccountingPeriodStartDates)
    {
        if (Year < 2020 || Year > 2050)
        {
            throw new InvalidOperationException();
        }
        if (Month <= 0 || Month > 12)
        {
            throw new InvalidOperationException();
        }
        if (existingAccountingPeriodStartDates.Count == 0)
        {
            return;
        }
        // Validate that there are no duplicate accounting periods
        if (existingAccountingPeriodStartDates.Any(period => period == PeriodStartDate))
        {
            throw new InvalidOperationException();
        }
        // Validate that accounting periods can only be added after existing accounting periods
        DateOnly previousMonth = PeriodStartDate.AddMonths(-1);
        if (!existingAccountingPeriodStartDates.Any(period => period == previousMonth))
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Validates closing this Accounting Period
    /// </summary>
    /// <param name="otherOpenAccountingPeriods">List of the Period Start Dates for any other open Accounting Periods</param>
    /// <param name="allAccountBalances">Ending Account Balances for every Account at the end of this Accounting Period</param>
    private void ValidateCloseAccountingPeriod(
        IEnumerable<DateOnly> otherOpenAccountingPeriods,
        IEnumerable<AccountBalanceByAccountingPeriod> allAccountBalances)
    {
        if (!IsOpen)
        {
            throw new InvalidOperationException();
        }
        if (otherOpenAccountingPeriods.Any(openPeriodStartDate => openPeriodStartDate < PeriodStartDate))
        {
            // We should always have a contiguous group of open accounting periods.
            // Only close the earliest open accounting period
            throw new InvalidOperationException();
        }
        // Validate that there are no pending balance changes in this Accounting Period
        if (allAccountBalances.Any(balance => balance.EndingBalance.PendingFundBalanceChanges.Count != 0))
        {
            throw new InvalidOperationException();
        }
    }
}