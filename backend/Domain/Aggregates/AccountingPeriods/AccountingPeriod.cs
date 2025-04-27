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
    public DateOnly PeriodStartDate => new(Year, Month, 1);

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
}