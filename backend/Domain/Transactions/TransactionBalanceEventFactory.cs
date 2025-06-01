using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Services;

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
            request.AccountId,
            request.EventType,
            request.AccountType);

    /// <inheritdoc/>
    protected override bool ValidatePrivate(CreateTransactionBalanceEventRequest request, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (request.Transaction.TransactionBalanceEvents.Any(balanceEvent =>
            balanceEvent.EventType == request.EventType && balanceEvent.AccountType == request.AccountType))
        {
            // Validate that we aren't creating a duplicate balance Event
            exception = new InvalidOperationException();
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
    /// Event Type for the Transaction Balance Event
    /// </summary>
    public required TransactionBalanceEventType EventType { get; init; }

    /// <summary>
    /// Account Type for the Transaction Balance Event
    /// </summary>
    public required TransactionAccountType AccountType { get; init; }
}