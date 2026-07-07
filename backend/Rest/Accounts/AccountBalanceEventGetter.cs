using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Class that retrieves balance events for a single Account workspace.
/// </summary>
public class AccountBalanceEventGetter(
    ITransactionRepository transactionRepository,
    AccountBalanceService accountBalanceService)
{
    /// <summary>
    /// Gets the paged balance events for the provided Account.
    /// </summary>
    public CollectionModel<AccountWorkspaceBalanceEventModel> Get(
        Account account,
        AccountBalanceEventQueryParameterModel request)
    {
        var balanceEvents = transactionRepository.GetAll()
            .SelectMany(transaction => BuildBalanceEvents(transaction, account))
            .OrderByDescending(balanceEvent => balanceEvent.Date)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
            .ThenByDescending(balanceEvent => balanceEvent.Sequence)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
            .ThenBy(balanceEvent => balanceEvent.Type)
            .ToList();

        return new CollectionModel<AccountWorkspaceBalanceEventModel>
        {
            Items = balanceEvents
                .Skip(request.Offset ?? 0)
                .Take(request.Limit ?? int.MaxValue)
                .Select(balanceEvent => balanceEvent.Model)
                .ToList(),
            TotalCount = balanceEvents.Count,
        };
    }

    private IEnumerable<AccountBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        Account account)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                if (spendingTransaction.Source.Account.Id == account.Id)
                {
                    yield return BuildBalanceEvent(
                        transaction,
                        account,
                        spendingTransaction.Source.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        spendingTransaction.Source.PostedDate != null,
                        transaction.Amount);
                }

                foreach (SpendingTransactionDestination destination in spendingTransaction.Destinations)
                {
                    if (destination.Account?.Id == account.Id)
                    {
                        yield return BuildBalanceEvent(
                            transaction,
                            account,
                            destination.PostedDate ?? transaction.Date,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            destination.Amount);
                    }
                }

                yield break;
            case IncomeTransaction incomeTransaction:
                if (incomeTransaction.Source.Account?.Id == account.Id)
                {
                    yield return BuildBalanceEvent(
                        transaction,
                        account,
                        incomeTransaction.Source.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        incomeTransaction.Source.PostedDate != null,
                        transaction.Amount);
                }

                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    if (destination.Account.Id == account.Id)
                    {
                        yield return BuildBalanceEvent(
                            transaction,
                            account,
                            destination.PostedDate ?? transaction.Date,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            destination.Amount);
                    }
                }

                yield break;
            case AccountTransaction accountTransaction:
                if (accountTransaction.Source.Account?.Id == account.Id)
                {
                    yield return BuildBalanceEvent(
                        transaction,
                        account,
                        accountTransaction.Source.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        accountTransaction.Source.PostedDate != null,
                        transaction.Amount);
                }

                foreach (AccountTransactionDestination destination in accountTransaction.Destinations)
                {
                    if (destination.Account?.Id == account.Id)
                    {
                        yield return BuildBalanceEvent(
                            transaction,
                            account,
                            destination.PostedDate ?? transaction.Date,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            destination.Amount);
                    }
                }

                yield break;
            default:
                yield break;
        }
    }

    private AccountBalanceEventRow BuildBalanceEvent(
        Transaction transaction,
        Account account,
        DateOnly date,
        AccountTrendsBalanceEventTypeModel type,
        bool isPosted,
        decimal amount)
    {
        AccountBalance previousBalance = accountBalanceService
            .GetPreviousBalanceForTransaction(transaction, account.Id);
        AccountBalance newBalance = accountBalanceService
            .GetNewBalanceForTransaction(transaction, account.Id);

        return new AccountBalanceEventRow(
            new AccountWorkspaceBalanceEventModel
            {
                TransactionId = transaction.Id.Value,
                Date = date,
                Type = type,
                IsPosted = isPosted,
                Amount = amount,
                PreviousBalance = previousBalance.PostedBalance,
                NewBalance = newBalance.PostedBalance,
            },
            date,
            transaction.Date,
            transaction.Sequence,
            transaction.Id.Value,
            type);
    }

    private sealed record AccountBalanceEventRow(
        AccountWorkspaceBalanceEventModel Model,
        DateOnly Date,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId,
        AccountTrendsBalanceEventTypeModel Type);
}