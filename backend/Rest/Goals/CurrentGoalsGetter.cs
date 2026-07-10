using Data.Goals;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Class that handles retrieving current Goal data.
/// </summary>
public class CurrentGoalsGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    AssignmentGoalRepository assignmentGoalRepository,
    SpendingGoalRepository spendingGoalRepository)
{
    /// <summary>
    /// Retrieves the current Goals page data.
    /// </summary>
    public CurrentGoalsModel Get(CurrentGoalsQueryParameterModel request)
    {
        AccountingPeriod? accountingPeriod = request.AccountingPeriodId is Guid accountingPeriodId
            ? accountingPeriodRepository.GetAll().FirstOrDefault(period => period.Id.Value == accountingPeriodId)
            : accountingPeriodRepository.GetLatestAccountingPeriod();
        if (accountingPeriod is null)
        {
            return CreateEmptyResult();
        }

        HashSet<Guid>? requestedFundIds = request.FundIds is { Count: > 0 }
            ? request.FundIds.ToHashSet()
            : null;

        var assignmentGoals = assignmentGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id)
            .ToList();
        var spendingGoals = spendingGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id)
            .ToList();

        var availableFundNames = assignmentGoals
            .Select(goal => goal.Fund.Name)
            .Concat(spendingGoals.Select(goal => goal.Fund.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assignmentGoalsByFundId = assignmentGoals
            .ToDictionary(goal => goal.Fund.Id.Value);
        var spendingGoalsByFundId = spendingGoals
            .ToDictionary(goal => goal.Fund.Id.Value);

        var includedFundIds = assignmentGoals
            .Where(goal => requestedFundIds == null || requestedFundIds.Contains(goal.Fund.Id.Value))
            .Select(goal => goal.Fund.Id.Value)
            .Concat(
                spendingGoals
                    .Where(goal => requestedFundIds == null || requestedFundIds.Contains(goal.Fund.Id.Value))
                    .Select(goal => goal.Fund.Id.Value))
            .Distinct()
            .OrderBy(
                fundId => assignmentGoalsByFundId.TryGetValue(fundId, out AssignmentGoal? assignmentGoal)
                    ? assignmentGoal.Fund.Name
                    : spendingGoalsByFundId[fundId].Fund.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        IReadOnlyCollection<Transaction> transactions = transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
        var assignmentEventsByFundId = transactions
            .SelectMany(BuildAssignmentEvents)
            .GroupBy(balanceEvent => balanceEvent.FundId)
            .ToDictionary(grouping => grouping.Key, grouping => SortBalanceEvents(grouping.ToList()));
        var spendingEventsByFundId = transactions
            .SelectMany(BuildSpendingEvents)
            .GroupBy(balanceEvent => balanceEvent.FundId)
            .ToDictionary(grouping => grouping.Key, grouping => SortBalanceEvents(grouping.ToList()));

        var includedAssignmentGoals = includedFundIds
            .Select(fundId => assignmentGoalsByFundId.GetValueOrDefault(fundId))
            .Where(goal => goal is not null)
            .Cast<AssignmentGoal>()
            .ToList();
        var includedSpendingGoals = includedFundIds
            .Select(fundId => spendingGoalsByFundId.GetValueOrDefault(fundId))
            .Where(goal => goal is not null)
            .Cast<SpendingGoal>()
            .ToList();

        int metAssignmentGoals = includedAssignmentGoals.Count(goal => goal.IsGoalMet);
        int metSpendingGoals = includedSpendingGoals.Count(goal => goal.IsGoalMet);

        return new CurrentGoalsModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            AvailableFundNames = availableFundNames,
            Summary = new CurrentGoalsSummaryModel
            {
                TotalAmountToAssign = includedAssignmentGoals.Sum(goal => goal.TotalAmountToAssign),
                TotalAmountAssigned = includedAssignmentGoals.Sum(goal => goal.TotalAmountAssigned),
                PercentageOfAssignmentGoalsMet = new GoalPercentageMetModel
                {
                    TotalCount = includedAssignmentGoals.Count,
                    MetCount = metAssignmentGoals,
                    PercentageMet = includedAssignmentGoals.Count == 0
                        ? 0
                        : metAssignmentGoals * 100m / includedAssignmentGoals.Count,
                },
                TotalAmountToSpend = includedSpendingGoals.Sum(goal => goal.TotalAmountToSpend),
                TotalAmountSpent = includedSpendingGoals.Sum(goal => goal.TotalAmountSpent),
                PercentageOfSpendingGoalsMet = new GoalPercentageMetModel
                {
                    TotalCount = includedSpendingGoals.Count,
                    MetCount = metSpendingGoals,
                    PercentageMet = includedSpendingGoals.Count == 0
                        ? 0
                        : metSpendingGoals * 100m / includedSpendingGoals.Count,
                },
            },
            Goals = includedFundIds
                .Select(fundId =>
                {
                    AssignmentGoal? assignmentGoal = assignmentGoalsByFundId.GetValueOrDefault(fundId);
                    SpendingGoal? spendingGoal = spendingGoalsByFundId.GetValueOrDefault(fundId);
                    string fundName = assignmentGoal?.Fund.Name ?? spendingGoal?.Fund.Name ?? string.Empty;

                    return new CurrentGoalModel
                    {
                        FundId = fundId,
                        FundName = fundName,
                        AssignmentGoal = assignmentGoal is null
                            ? null
                            : ToProgressModel(
                                assignmentGoal.Id.Value,
                                assignmentGoal.TotalAmountToAssign,
                                assignmentGoal.TotalAmountAssigned,
                                Math.Max(assignmentGoal.TotalAmountToAssign - assignmentGoal.TotalAmountAssigned, 0),
                                assignmentGoal.IsGoalMet,
                                assignmentEventsByFundId.GetValueOrDefault(fundId) ?? []),
                        SpendingGoal = spendingGoal is null
                            ? null
                            : ToProgressModel(
                                spendingGoal.Id.Value,
                                spendingGoal.TotalAmountToSpend,
                                spendingGoal.TotalAmountSpent,
                                spendingGoal.TotalAmountToSpend - spendingGoal.TotalAmountSpent,
                                spendingGoal.IsGoalMet,
                                spendingEventsByFundId.GetValueOrDefault(fundId) ?? []),
                    };
                })
                .ToList(),
        };
    }

    private static CurrentGoalsModel CreateEmptyResult() => new()
    {
        AccountingPeriodId = null,
        AccountingPeriodName = null,
        AvailableFundNames = [],
        Summary = new CurrentGoalsSummaryModel
        {
            TotalAmountToAssign = 0,
            TotalAmountAssigned = 0,
            PercentageOfAssignmentGoalsMet = new GoalPercentageMetModel
            {
                TotalCount = 0,
                MetCount = 0,
                PercentageMet = 0,
            },
            TotalAmountToSpend = 0,
            TotalAmountSpent = 0,
            PercentageOfSpendingGoalsMet = new GoalPercentageMetModel
            {
                TotalCount = 0,
                MetCount = 0,
                PercentageMet = 0,
            },
        },
        Goals = [],
    };

    private static CurrentGoalProgressModel ToProgressModel(
        Guid goalId,
        decimal targetAmount,
        decimal currentAmount,
        decimal remainingAmount,
        bool isGoalMet,
        IReadOnlyList<CurrentGoalBalanceEventRow> balanceEvents) => new()
        {
            GoalId = goalId,
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
            RemainingAmount = remainingAmount,
            IsGoalMet = isGoalMet,
            LastBalanceEventDate = balanceEvents.Count > 0 ? balanceEvents[0].Date : null,
            RecentBalanceEvents = balanceEvents
                .Take(5)
                .Select(ToModel)
                .ToList(),
        };

    private static List<CurrentGoalBalanceEventRow> SortBalanceEvents(
        IReadOnlyList<CurrentGoalBalanceEventRow> balanceEvents) => balanceEvents
        .OrderByDescending(balanceEvent => balanceEvent.Date)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
        .ThenByDescending(balanceEvent => balanceEvent.Sequence)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
        .ToList();

    private static IEnumerable<CurrentGoalBalanceEventRow> BuildAssignmentEvents(
        Transaction transaction)
    {
        if (transaction is not IncomeTransaction incomeTransaction)
        {
            yield break;
        }

        foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
        {
            foreach (FundAmount fundAssignment in destination.FundAssignments)
            {
                yield return new CurrentGoalBalanceEventRow(
                    fundAssignment.FundId.Value,
                    destination.PostedDate ?? incomeTransaction.Date,
                    destination.PostedDate != null,
                    fundAssignment.Amount,
                    transaction.Date,
                    transaction.Sequence,
                    transaction.Id.Value);
            }
        }
    }

    private static IEnumerable<CurrentGoalBalanceEventRow> BuildSpendingEvents(
        Transaction transaction)
    {
        if (transaction is not SpendingTransaction spendingTransaction)
        {
            yield break;
        }

        foreach (SpendingTransactionDestination destination in spendingTransaction.Destinations)
        {
            foreach (FundAmount fundAssignment in destination.FundAssignments)
            {
                yield return new CurrentGoalBalanceEventRow(
                    fundAssignment.FundId.Value,
                    spendingTransaction.Date,
                    true,
                    fundAssignment.Amount,
                    transaction.Date,
                    transaction.Sequence,
                    transaction.Id.Value);
            }
        }
    }

    private static CurrentGoalBalanceEventModel ToModel(
        CurrentGoalBalanceEventRow row) => new()
        {
            TransactionId = row.TransactionId,
            Date = row.Date,
            IsPosted = row.IsPosted,
            Amount = row.Amount,
        };

    private sealed record CurrentGoalBalanceEventRow(
        Guid FundId,
        DateOnly Date,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}