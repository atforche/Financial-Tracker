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
    /// <param name="transactionDate">Date for this Transaction</param>
    /// <param name="accountingEntries">Accounting Entries for this Transaction</param>
    /// <param name="transactionBalanceEvents">Requests to create the Transaction Balance Events for this Transaction</param>
    internal Transaction(AccountingPeriod accountingPeriod,
        DateOnly transactionDate,
        IEnumerable<FundAmount> accountingEntries,
        IEnumerable<CreateTransactionBalanceEventRequest> transactionBalanceEvents)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        TransactionDate = transactionDate;
        _accountingEntries = accountingEntries.ToList();
        _transactionBalanceEvents = transactionBalanceEvents.Select(request => new TransactionBalanceEvent(this,
            request.Account,
            request.EventDate,
            request.EventSequence,
            request.TransactionEventType,
            request.TransactionAccountType)).ToList();
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

/// <summary>
/// Record representing a request to create a Transaction Balance Event
/// </summary>
internal sealed record CreateTransactionBalanceEventRequest
{
    /// <inheritdoc cref="TransactionBalanceEvent.Account"/>
    public required Account Account { get; init; }

    /// <inheritdoc cref="TransactionBalanceEvent.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="TransactionBalanceEvent.EventSequence"/>
    public required int EventSequence { get; init; }

    /// <inheritdoc cref="TransactionBalanceEvent.TransactionEventType"/>
    public required TransactionBalanceEventType TransactionEventType { get; init; }

    /// <inheritdoc cref="TransactionBalanceEvent.TransactionAccountType"/>
    public required TransactionAccountType TransactionAccountType { get; init; }
}