using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Class that retrieves balance events for a single Fund workspace.
/// </summary>
public class FundBalanceEventGetter(
    ITransactionRepository transactionRepository,
    FundBalanceService fundBalanceService)
{
    /// <summary>
    /// Gets the paged balance events for the provided Fund.
    /// </summary>
    public CollectionModel<FundWorkspaceBalanceEventModel> Get(
        Fund fund,
        FundBalanceEventQueryParameterModel request)
    {
        var balanceEvents = transactionRepository.GetAll()
            .SelectMany(transaction => BuildBalanceEvents(transaction, fund))
            .OrderByDescending(balanceEvent => balanceEvent.Date)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
            .ThenByDescending(balanceEvent => balanceEvent.Sequence)
            .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
            .ThenBy(balanceEvent => balanceEvent.Type)
            .ToList();

        return new CollectionModel<FundWorkspaceBalanceEventModel>
        {
            Items = balanceEvents
                .Skip(request.Offset ?? 0)
                .Take(request.Limit ?? int.MaxValue)
                .Select(balanceEvent => balanceEvent.Model)
                .ToList(),
            TotalCount = balanceEvents.Count,
        };
    }

    private IEnumerable<FundBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        Fund fund)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (FundAmount fundAssignment in spendingTransaction.Destinations
                    .SelectMany(destination => destination.FundAssignments)
                    .Where(fundAssignment => fundAssignment.FundId == fund.Id))
                {
                    yield return BuildBalanceEvent(transaction, fund, spendingTransaction.Date,
                        FundTrendsBalanceEventTypeModel.Debit, true, fundAssignment.Amount);
                }
                yield break;
            case IncomeTransaction incomeTransaction:
                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    foreach (FundAmount fundAssignment in destination.FundAssignments
                        .Where(fundAssignment => fundAssignment.FundId == fund.Id))
                    {
                        yield return BuildBalanceEvent(transaction, fund,
                            destination.PostedDate ?? incomeTransaction.Date,
                            FundTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null, fundAssignment.Amount);
                    }
                }
                yield break;
            case FundTransaction fundTransaction:
                if (fundTransaction.Source.Fund.Id == fund.Id)
                {
                    yield return BuildBalanceEvent(transaction, fund, fundTransaction.Date,
                        FundTrendsBalanceEventTypeModel.Debit, true, fundTransaction.Amount);
                }
                foreach (FundTransactionDestination destination in fundTransaction.Destinations
                    .Where(destination => destination.Fund.Id == fund.Id))
                {
                    yield return BuildBalanceEvent(transaction, fund, fundTransaction.Date,
                        FundTrendsBalanceEventTypeModel.Credit, true, destination.Amount);
                }
                yield break;
            default:
                yield break;
        }
    }

    private FundBalanceEventRow BuildBalanceEvent(
        Transaction transaction,
        Fund fund,
        DateOnly date,
        FundTrendsBalanceEventTypeModel type,
        bool isPosted,
        decimal amount)
    {
        FundBalance previousBalance = fundBalanceService.GetPreviousBalancesForTransaction(transaction)
            .Single(balance => balance.FundId == fund.Id);
        FundBalance newBalance = fundBalanceService.GetNewBalanceForTransaction(transaction)
            .Single(balance => balance.FundId == fund.Id);

        return new FundBalanceEventRow(
            new FundWorkspaceBalanceEventModel
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

    private sealed record FundBalanceEventRow(
        FundWorkspaceBalanceEventModel Model,
        DateOnly Date,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId,
        FundTrendsBalanceEventTypeModel Type);
}