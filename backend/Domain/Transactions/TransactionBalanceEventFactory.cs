using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;

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
    protected override bool ValidatePrivate(CreateTransactionBalanceEventRequest request, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (request.Transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.EventDate == request.EventDate))
        {
            // Validate that multiple Transaction Balance Events can't exist for the same date
            exception = new InvalidOperationException();
        }
        if (request.Transaction.DebitAccountId == null &&
            (request.Parts.Contains(TransactionBalanceEventPartType.AddedDebit) || request.Parts.Contains(TransactionBalanceEventPartType.PostedDebit)))
        {
            // Validate that the Transaction must have a debit account for the balance event to have debit account parts
            exception ??= new InvalidOperationException();
        }
        if (request.Transaction.CreditAccountId == null &&
            (request.Parts.Contains(TransactionBalanceEventPartType.AddedCredit) || request.Parts.Contains(TransactionBalanceEventPartType.PostedCredit)))
        {
            // Validate that the Transaction must have a credit account for the balance event to have credit account parts
            exception ??= new InvalidOperationException();
        }
        return exception == null;
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