using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.BalanceEvents.Exceptions;
using Domain.Transactions.Exceptions;
namespace Domain.Transactions;

/// <summary>
/// Factory for builder a <see cref="TransactionBalanceEvent"/>
/// </summary>
public class TransactionBalanceEventFactory(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IBalanceEventRepository balanceEventRepository,
    AccountBalanceService accountBalanceService)
    : BalanceEventFactory<TransactionBalanceEvent, CreateTransactionBalanceEventRequest>(
        accountingPeriodRepository,
        accountRepository,
        balanceEventRepository,
        accountBalanceService)
{
    /// <inheritdoc/>
    protected override TransactionBalanceEvent CreatePrivate(CreateTransactionBalanceEventRequest request) =>
        new(request.Transaction,
            request.EventDate,
            GetBalanceEventSequence(request.EventDate),
            request.Parts);

    /// <inheritdoc/>
    protected override bool ValidatePrivate(CreateTransactionBalanceEventRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (request.Transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.EventDate == request.EventDate))
        {
            // Validate that multiple Transaction Balance Events can't exist for the same date
            exceptions = exceptions.Append(new InvalidEventDateException("A Transaction Balance Event already exists for the specified event date."));
        }
        if (request.Transaction.DebitAccountId == null &&
            (request.Parts.Contains(TransactionBalanceEventPartType.AddedDebit) || request.Parts.Contains(TransactionBalanceEventPartType.PostedDebit)))
        {
            // Validate that the Transaction must have a debit account for the balance event to have debit account parts
            exceptions = exceptions.Append(new InvalidBalanceEventPartException("A Transaction without a debit account cannot have a debit balance event part."));
        }
        if (request.Transaction.CreditAccountId == null &&
            (request.Parts.Contains(TransactionBalanceEventPartType.AddedCredit) || request.Parts.Contains(TransactionBalanceEventPartType.PostedCredit)))
        {
            // Validate that the Transaction must have a credit account for the balance event to have credit account parts
            exceptions = exceptions.Append(new InvalidBalanceEventPartException("A Transaction without a credit account cannot have a credit balance event part."));
        }
        return !exceptions.Any();
    }
}

/// <summary>
/// Record representing a request to create a <see cref="TransactionBalanceEvent"/>
/// </summary>
public record CreateTransactionBalanceEventRequest : CreateBalanceEventRequest
{
    /// <summary>
    /// Transaction for the Transaction Balance Event
    /// </summary>
    public required Transaction Transaction { get; init; }

    /// <summary>
    /// Transaction Balance Event Parts for the Transaction Balance Event
    /// </summary>
    public required IReadOnlyCollection<TransactionBalanceEventPartType> Parts { get; init; }

    /// <inheritdoc/>
    public override IReadOnlyCollection<AccountId> GetAccountIds()
    {
        List<AccountId> results = [];
        if (Parts.Contains(TransactionBalanceEventPartType.AddedDebit) || Parts.Contains(TransactionBalanceEventPartType.PostedDebit))
        {
            results.Add(Transaction.DebitAccountId ?? throw new InvalidOperationException());
        }
        if (Parts.Contains(TransactionBalanceEventPartType.AddedCredit) || Parts.Contains(TransactionBalanceEventPartType.PostedCredit))
        {
            results.Add(Transaction.CreditAccountId ?? throw new InvalidOperationException());
        }
        return results;
    }
}