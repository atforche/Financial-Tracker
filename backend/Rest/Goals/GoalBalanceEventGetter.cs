using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Class that retrieves balance events for a single Goal workspace.
/// </summary>
public class GoalBalanceEventGetter(ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Gets the paged balance events for the provided Fund and Accounting Period.
    /// </summary>
    public CollectionModel<GoalWorkspaceBalanceEventModel> Get(
        Fund fund,
        AccountingPeriod accountingPeriod,
        GoalBalanceEventQueryParameterModel request)
    {
        var balanceEvents = transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id)
            .SelectMany(transaction => BuildBalanceEvents(transaction, fund))
            .OrderByDescending(balanceEvent => balanceEvent.Date)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
            .ThenByDescending(balanceEvent => balanceEvent.Sequence)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
            .ThenBy(balanceEvent => balanceEvent.Type)
            .ToList();

        return new CollectionModel<GoalWorkspaceBalanceEventModel>
        {
            Items = balanceEvents
                .Skip(request.Offset ?? 0)
                .Take(request.Limit ?? int.MaxValue)
                .Select(balanceEvent => balanceEvent.Model)
                .ToList(),
            TotalCount = balanceEvents.Count,
        };
    }

    private static IEnumerable<GoalBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        Fund fund)
    {
        switch (transaction)
        {
            case IncomeTransaction incomeTransaction:
                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    foreach (FundAmount fundAssignment in destination.FundAssignments
                        .Where(assignment => assignment.FundId == fund.Id))
                    {
                        yield return BuildBalanceEvent(
                            transaction,
                            destination.PostedDate ?? incomeTransaction.Date,
                            GoalWorkspaceBalanceEventTypeModel.Assignment,
                            destination.PostedDate != null,
                            fundAssignment.Amount);
                    }
                }
                yield break;
            case SpendingTransaction spendingTransaction:
                foreach (FundAmount fundAssignment in spendingTransaction.Destinations
                    .SelectMany(destination => destination.FundAssignments)
                    .Where(assignment => assignment.FundId == fund.Id))
                {
                    yield return BuildBalanceEvent(
                        transaction,
                        spendingTransaction.Date,
                        GoalWorkspaceBalanceEventTypeModel.Spending,
                        true,
                        fundAssignment.Amount);
                }
                yield break;
            default:
                yield break;
        }
    }

    private static GoalBalanceEventRow BuildBalanceEvent(
        Transaction transaction,
        DateOnly date,
        GoalWorkspaceBalanceEventTypeModel type,
        bool isPosted,
        decimal amount) => new(
            new GoalWorkspaceBalanceEventModel
            {
                TransactionId = transaction.Id.Value,
                Date = date,
                Type = type,
                IsPosted = isPosted,
                Amount = amount,
            },
            date,
            transaction.Date,
            transaction.Sequence,
            transaction.Id.Value,
            type);

    private sealed record GoalBalanceEventRow(
        GoalWorkspaceBalanceEventModel Model,
        DateOnly Date,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId,
        GoalWorkspaceBalanceEventTypeModel Type);
}