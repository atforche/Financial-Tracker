using Data.Funds;
using Data.Transactions;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving current Fund data.
/// </summary>
public class CurrentFundsGetter(
    FundRepository fundRepository,
    TransactionRepository transactionRepository,
    FundConverter fundConverter,
    FundSummaryGetter fundSummaryGetter)
{
    /// <summary>
    /// Retrieves the current Funds page data.
    /// </summary>
    public CurrentFundsModel Get(CurrentFundsQueryParameterModel request)
    {
        HashSet<string>? requestedFundNames = NormalizeNames(request.FundName);

        var baseFunds = fundRepository.GetAll()
            .OrderBy(fund => fund.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(fund => fund.Id.Value)
            .ToList();

        var availableFundNames = baseFunds
            .Select(fund => fund.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        HashSet<string>? applicableFundNames = GetApplicableNames(requestedFundNames, availableFundNames);
        var funds = baseFunds
            .Where(fund => applicableFundNames == null || applicableFundNames.Contains(fund.Name))
            .ToList();

        var balanceEventsByFundId = transactionRepository.GetAll()
            .SelectMany(BuildBalanceEvents)
            .GroupBy(balanceEvent => balanceEvent.FundId.Value)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => SortBalanceEvents(grouping.ToList()));

        return new CurrentFundsModel
        {
            AvailableFundNames = availableFundNames,
            Summary = fundSummaryGetter.Get(funds),
            Funds = funds
                .Select(fund => ToModel(
                    fund,
                    balanceEventsByFundId.GetValueOrDefault(fund.Id.Value) ?? []))
                .ToList(),
        };
    }

    private static HashSet<string>? NormalizeNames(IReadOnlyCollection<string>? names)
    {
        if (names is not { Count: > 0 })
        {
            return null;
        }

        var normalizedNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return normalizedNames.Count == 0 ? null : normalizedNames;
    }

    private static HashSet<string>? GetApplicableNames(
        IReadOnlySet<string>? requestedNames,
        IReadOnlyCollection<string> availableNames)
    {
        if (requestedNames == null)
        {
            return null;
        }

        var applicableNames = availableNames
            .Where(requestedNames.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return applicableNames.Count == 0 ? null : applicableNames;
    }

    private CurrentFundModel ToModel(
        Fund fund,
        IReadOnlyList<CurrentFundBalanceEventRow> balanceEvents)
    {
        FundModel fundModel = fundConverter.ToModel(fund);

        return new CurrentFundModel
        {
            Id = fundModel.Id,
            Name = fundModel.Name,
            CurrentBalance = fundModel.CurrentBalance,
            LastBalanceEventDate = balanceEvents.Count > 0 ? balanceEvents[0].Date : null,
            RecentBalanceEvents = balanceEvents
                .Take(5)
                .Select(ToModel)
                .ToList(),
        };
    }

    private static List<CurrentFundBalanceEventRow> SortBalanceEvents(
        IReadOnlyList<CurrentFundBalanceEventRow> balanceEvents) => balanceEvents
        .OrderByDescending(balanceEvent => balanceEvent.Date)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
        .ThenByDescending(balanceEvent => balanceEvent.Sequence)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
        .ThenBy(balanceEvent => balanceEvent.Type)
        .ToList();

    private static IEnumerable<CurrentFundBalanceEventRow> BuildBalanceEvents(
        Transaction transaction)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (SpendingTransactionDestination destination in spendingTransaction.Destinations)
                {
                    foreach (FundAmount fundAssignment in destination.FundAssignments)
                    {
                        yield return new CurrentFundBalanceEventRow(
                            fundAssignment.FundId,
                            spendingTransaction.Date,
                            FundTrendsBalanceEventTypeModel.Debit,
                            true,
                            fundAssignment.Amount,
                            transaction.Date,
                            transaction.Sequence,
                            transaction.Id.Value);
                    }
                }

                yield break;
            case IncomeTransaction incomeTransaction:
                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    foreach (FundAmount fundAssignment in destination.FundAssignments)
                    {
                        yield return new CurrentFundBalanceEventRow(
                            fundAssignment.FundId,
                            destination.PostedDate ?? incomeTransaction.Date,
                            FundTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            fundAssignment.Amount,
                            transaction.Date,
                            transaction.Sequence,
                            transaction.Id.Value);
                    }
                }

                yield break;
            default:
                yield break;
        }
    }

    private static CurrentFundBalanceEventModel ToModel(
        CurrentFundBalanceEventRow balanceEvent) => new()
        {
            TransactionId = balanceEvent.TransactionId,
            Date = balanceEvent.Date,
            Type = balanceEvent.Type,
            IsPosted = balanceEvent.IsPosted,
            Amount = balanceEvent.Amount,
        };

    private sealed record CurrentFundBalanceEventRow(
        FundId FundId,
        DateOnly Date,
        FundTrendsBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}