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
    private readonly List<FundAmount>? _debitFundAmounts;
    private readonly List<FundAmount>? _creditFundAmounts;
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
    /// Debit Account ID for this Transaction
    /// </summary>
    public AccountId? DebitAccountId { get; private set; }

    /// <summary>
    /// Debit Fund Amounts for this Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount>? DebitFundAmounts => _debitFundAmounts;

    /// <summary>
    /// Credit Account ID for this Transaction
    /// </summary>
    public AccountId? CreditAccountId { get; private set; }

    /// <summary>
    /// Credit Fund Amounts for this Transaction
    /// </summary>
    public IReadOnlyCollection<FundAmount>? CreditFundAmounts => _creditFundAmounts;

    /// <summary>
    /// List of Balance Events for this Transaction
    /// </summary>
    public IReadOnlyCollection<TransactionBalanceEvent> TransactionBalanceEvents => _transactionBalanceEvents;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Transaction</param>
    /// <param name="eventDate">Date for this Transaction</param>
    /// <param name="debitAccountId">Debit Account ID for this Transaction</param>
    /// <param name="debitFundAmounts">Debit Fund Amounts for this Transaction</param>
    /// <param name="creditAccountId">Credit Account ID for this Transaction</param>
    /// <param name="creditFundAmounts">Credit Fund Amounts for this Transaction</param>
    internal Transaction(AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        AccountId? debitAccountId,
        IEnumerable<FundAmount>? debitFundAmounts,
        AccountId? creditAccountId,
        IEnumerable<FundAmount>? creditFundAmounts)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriodId = accountingPeriodId;
        Date = eventDate;
        DebitAccountId = debitAccountId;
        _debitFundAmounts = debitFundAmounts?.ToList();
        CreditAccountId = creditAccountId;
        _creditFundAmounts = creditFundAmounts?.ToList();
        _transactionBalanceEvents = [];
    }

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
        _debitFundAmounts = null!;
        _creditFundAmounts = null!;
        _transactionBalanceEvents = [];
    }
}