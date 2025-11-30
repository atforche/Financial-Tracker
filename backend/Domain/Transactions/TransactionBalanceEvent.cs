using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a Transaction Balance Event
/// </summary>
/// <remarks>
/// A Transaction will generate balance events for both the credited and debited
/// accounts. For each account, a balance event will be generated when the 
/// Transaction is added and when the Transaction is posted.
/// </remarks>
public sealed class TransactionBalanceEvent : Entity<TransactionBalanceEventId>, IBalanceEvent
{
    private readonly List<TransactionBalanceEventPart> _parts = [];

    /// <summary>
    /// Parent Transaction for this Transaction Balance Event
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <inheritdoc/>
    public AccountingPeriodId AccountingPeriodId => Transaction.AccountingPeriodId;

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Transaction Balance Event Parts for this Transaction Balance Event
    /// </summary>
    public IReadOnlyCollection<TransactionBalanceEventPart> Parts => _parts;

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountId> GetAccountIds() => Parts.Select(part => part.AccountId).Distinct().ToList();

    /// <inheritdoc/>
    public bool IsValidToApplyToBalance(AccountBalance currentBalance, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        foreach (TransactionBalanceEventPart part in Parts)
        {
            if (!part.IsValidToApplyToBalance(currentBalance, out IEnumerable<Exception> partExceptions))
            {
                exceptions = exceptions.Concat(partExceptions);
            }
        }
        return !exceptions.Any();
    }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance, ApplicationDirection direction)
    {
        foreach (TransactionBalanceEventPart part in Parts)
        {
            currentBalance = part.ApplyEventPartToBalance(currentBalance, direction);
        }
        return currentBalance;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transaction">Parent Transaction for this Transaction Balance Event</param>
    /// <param name="eventDate">Event Date for this Transaction Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Transaction Balance Event</param>
    /// <param name="parts">Transaction Balance Event Parts for this Transaction Balance Event</param>
    internal TransactionBalanceEvent(
        Transaction transaction,
        DateOnly eventDate,
        int eventSequence,
        IEnumerable<TransactionBalanceEventPartType> parts)
        : base(new TransactionBalanceEventId(Guid.NewGuid()))
    {
        Transaction = transaction;
        EventDate = eventDate;
        EventSequence = eventSequence;
        foreach (TransactionBalanceEventPartType part in parts)
        {
            AddPart(new TransactionBalanceEventPart(this, part));
        }
    }

    /// <summary>
    /// Adds a Transaction Balance Event Part to this Transaction Balance Event
    /// </summary>
    /// <param name="part">Transaction Balance Event Part to add to this Transaction Balance Event</param>
    internal void AddPart(TransactionBalanceEventPart part) => _parts.Add(part);

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionBalanceEvent() : base() => Transaction = null!;
}

/// <summary>
/// Value object class representing the ID of an <see cref="TransactionBalanceEvent"/>
/// </summary>
public record TransactionBalanceEventId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Transaction Balance Event ID during 
    /// Transaction creation. 
    /// </summary>
    /// <param name="value">Value for this Transaction Balance Event ID</param>
    internal TransactionBalanceEventId(Guid value)
        : base(value)
    {
    }
}