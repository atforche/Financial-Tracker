using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

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
public class Transaction : Entity<TransactionId>
{
    private readonly List<FundAmount> _fundAmounts;
    private readonly List<TransactionBalanceEvent> _transactionBalanceEvents;

    /// <summary>
    /// Accounting Period ID for this Transaction
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// List of Fund Amounts for this Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <summary>
    /// List of Balance Events for this Transaction
    /// </summary>
    public IReadOnlyCollection<TransactionBalanceEvent> TransactionBalanceEvents => _transactionBalanceEvents;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Transaction</param>
    /// <param name="eventDate">Date for this Transaction</param>
    /// <param name="fundAmounts">Fund Amounts for this Transaction</param>
    internal Transaction(AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        IEnumerable<FundAmount> fundAmounts)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriodId = accountingPeriodId;
        Date = eventDate;
        _fundAmounts = fundAmounts.ToList();
        _transactionBalanceEvents = [];
    }

    /// <summary>
    /// Gets the Account ID for this Transaction that corresponds to the provided Transaction Account Type
    /// </summary>
    /// <param name="accountType">Type of the Account to get</param>
    /// <returns>The Account ID for this Transaction that corresponds to the provided type</returns>
    internal AccountId? GetAccountId(TransactionAccountType accountType) =>
        TransactionBalanceEvents.SingleOrDefault(balanceEvent =>
            balanceEvent.AccountType == accountType &&
            balanceEvent.EventType == TransactionBalanceEventType.Added)?.AccountId;

    /// <summary>
    /// Adds a Transaction Balance Event to this Transaction
    /// </summary>
    /// <param name="transactionBalanceEvent">Transaction Balance Event</param>
    internal void AddBalanceEvent(TransactionBalanceEvent transactionBalanceEvent) =>
        _transactionBalanceEvents.Add(transactionBalanceEvent);

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction() : base()
    {
        AccountingPeriodId = null!;
        _fundAmounts = [];
        _transactionBalanceEvents = [];
    }
}