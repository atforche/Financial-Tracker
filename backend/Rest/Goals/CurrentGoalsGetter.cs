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
    public CurrentGoalsModel Get()
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (accountingPeriod is null)
        {
            return CreateEmptyResult();
        }

        var assignmentGoals = assignmentGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id)
            .ToDictionary(goal => goal.Fund.Id.Value);
        var spendingGoals = spendingGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id)
            .ToDictionary(goal => goal.Fund.Id.Value);

        IReadOnlyCollection<Transaction> transactions = transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
        var assignmentEventsByFundId = transactions
            .SelectMany(BuildAssignmentEvents)
            .GroupBy(balanceEvent => balanceEvent.FundId)
            .ToDictionary(grouping => grouping.Key, grouping => SortBalanceEvents(grouping.ToList()));
        var spendingEventsByFundId = transactions
            .SelectMany(BuildSpendingEvents)
            .GroupBy(balanceEvent => balanceEvent.FundId)
            .ToDictionary(grouping => grouping.Key, grouping => SortBalanceEvents(grouping.ToList()));

        var fundIds = assignmentGoals.Keys
            .Concat(spendingGoals.Keys)
            .Distinct()
            .OrderBy(
                fundId => assignmentGoals.TryGetValue(fundId, out AssignmentGoal? assignmentGoal)
                    ? assignmentGoal.Fund.Name
                    : spendingGoals[fundId].Fund.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        var assignmentGoalList = assignmentGoals.Values
            .OrderBy(goal => goal.Fund.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var spendingGoalList = spendingGoals.Values
            .OrderBy(goal => goal.Fund.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        int metAssignmentGoals = assignmentGoalList.Count(goal => goal.IsGoalMet);
        int metSpendingGoals = spendingGoalList.Count(goal => goal.IsGoalMet);

        return new CurrentGoalsModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Summary = new CurrentGoalsSummaryModel
            {
                TotalAmountToAssign = assignmentGoalList.Sum(goal => goal.TotalAmountToAssign),
                TotalAmountAssigned = assignmentGoalList.Sum(goal => goal.TotalAmountAssigned),
                PercentageOfAssignmentGoalsMet = new GoalPercentageMetModel
                {
                    TotalCount = assignmentGoalList.Count,
                    MetCount = metAssignmentGoals,
                    PercentageMet = assignmentGoalList.Count == 0
                        ? 0
                        : metAssignmentGoals * 100m / assignmentGoalList.Count,
                },
                TotalAmountToSpend = spendingGoalList.Sum(goal => goal.TotalAmountToSpend),
                TotalAmountSpent = spendingGoalList.Sum(goal => goal.TotalAmountSpent),
                PercentageOfSpendingGoalsMet = new GoalPercentageMetModel
                {
                    TotalCount = spendingGoalList.Count,
                    MetCount = metSpendingGoals,
                    PercentageMet = spendingGoalList.Count == 0
                        ? 0
                        : metSpendingGoals * 100m / spendingGoalList.Count,
                },
            },
            Goals = fundIds
                .Select(fundId =>
                {
                    AssignmentGoal? assignmentGoal = assignmentGoals.GetValueOrDefault(fundId);
                    SpendingGoal? spendingGoal = spendingGoals.GetValueOrDefault(fundId);
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
                                assignmentGoal.IsGoalMet,
                                assignmentEventsByFundId.GetValueOrDefault(fundId) ?? []),
                        SpendingGoal = spendingGoal is null
                            ? null
                            : ToProgressModel(
                                spendingGoal.Id.Value,
                                spendingGoal.TotalAmountToSpend,
                                spendingGoal.TotalAmountSpent,
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
        bool isGoalMet,
        IReadOnlyList<CurrentGoalBalanceEventRow> balanceEvents) => new()
        {
            GoalId = goalId,
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
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

        foreach (FundAmount fundAssignment in incomeTransaction.FundAssignments)
        {
            yield return new CurrentGoalBalanceEventRow(
                fundAssignment.FundId.Value,
                incomeTransaction.Date,
                true,
                fundAssignment.Amount,
                transaction.Date,
                transaction.Sequence,
                transaction.Id.Value);
        }
    }

    private static IEnumerable<CurrentGoalBalanceEventRow> BuildSpendingEvents(
        Transaction transaction)
    {
        if (transaction is not SpendingTransaction spendingTransaction)
        {
            yield break;
        }

        foreach (FundAmount fundAssignment in spendingTransaction.FundAssignments)
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