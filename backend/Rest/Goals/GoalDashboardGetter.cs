using Domain.AccountingPeriods;
using Domain.Goals;
using Models;
using Models.Goals;
using Rest.AccountingPeriods;

namespace Rest.Goals;

/// <summary>
/// Class that handles retrieving Goal dashboard data for an Accounting Period range.
/// </summary>
public class GoalDashboardGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IGoalRepository goalRepository,
    GoalConverter goalConverter,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Retrieves the Goal dashboard data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        GoalDashboardQueryParameterModel request,
        out GoalDashboardModel results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (request.StartAccountingPeriodId is null)
        {
            errors.Add(nameof(request.StartAccountingPeriodId), ["StartAccountingPeriodId is required."]);
        }

        if (request.EndAccountingPeriodId is null)
        {
            errors.Add(nameof(request.EndAccountingPeriodId), ["EndAccountingPeriodId is required."]);
        }

        if (errors.Count > 0 || request.StartAccountingPeriodId is null || request.EndAccountingPeriodId is null)
        {
            results = CreateEmptyResult();
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(request.StartAccountingPeriodId.Value, request.EndAccountingPeriodId.Value, out List<AccountingPeriod> accountingPeriods, out string? rangeError))
        {
            errors.Add(nameof(request.EndAccountingPeriodId), [rangeError ?? "The requested Accounting Period range is invalid."]);
            results = CreateEmptyResult();
            return false;
        }

        HashSet<GoalType>? requestedGoalTypes = null;
        if (request.GoalType is { Count: > 0 } requestedGoalTypesInput)
        {
            requestedGoalTypes = [];
            foreach (GoalTypeModel goalTypeModel in requestedGoalTypesInput)
            {
                if (!GoalTypeConverter.TryToDomain(goalTypeModel, out GoalType? goalType) || goalType is null)
                {
                    errors.Add(nameof(request.GoalType), [$"Unrecognized Goal Type: {goalTypeModel}"]);
                    continue;
                }

                GoalType goalTypeValue = goalType.Value;
                _ = requestedGoalTypes.Add(goalTypeValue);
            }
        }

        var requestedFundNames = NormalizeNames(request.FundName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var baseGoals = goalRepository.GetAll()
            .Where(goal => accountingPeriods.Any(accountingPeriod => accountingPeriod.Id == goal.AccountingPeriodId))
            .ToList();

        var availableFundNames = baseGoals
            .Select(goal => goal.Fund.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (errors.Count > 0)
        {
            results = CreateEmptyResult();
            return false;
        }

        IEnumerable<Goal> filteredGoals = baseGoals
            .Where(goal => requestedGoalTypes == null || requestedGoalTypes.Count == 0 || requestedGoalTypes.Contains(goal.GoalType))
            .Where(goal => requestedFundNames.Count == 0 || requestedFundNames.Contains(goal.Fund.Name));

        List<Goal> sortedGoals = SortGoals(filteredGoals.ToList(), request.Sort);

        var goalModels = sortedGoals
            .Select(goalConverter.ToModel)
            .ToList();

        decimal totalGoalAmount = sortedGoals.Sum(goal => goal.GoalAmount);
        decimal totalAmountAssigned = sortedGoals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        decimal totalAmountSpent = sortedGoals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metGoals = sortedGoals.Count(goal => goal.IsAssignmentGoalMet && goal.IsSpendingGoalMet);

        List<GoalDashboardGoalTypeSummaryModel> goalTypeSummary = BuildTypeSummary(sortedGoals);
        List<GoalDashboardAccountingPeriodSummaryModel> accountingPeriodSummary = BuildAccountingPeriodSummary(sortedGoals);

        results = new GoalDashboardModel
        {
            Goals = new CollectionModel<GoalModel>
            {
                Items = goalModels.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
                TotalCount = goalModels.Count,
            },
            AvailableFundNames = availableFundNames,
            TotalGoalAmount = totalGoalAmount,
            TotalAmountAssigned = totalAmountAssigned,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfGoalsMet = sortedGoals.Count == 0 ? 0 : metGoals * 100m / sortedGoals.Count,
            GoalTypes = goalTypeSummary,
            AccountingPeriods = accountingPeriodSummary,
        };

        return true;
    }

    private static GoalDashboardModel CreateEmptyResult() => new()
    {
        Goals = new CollectionModel<GoalModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AvailableFundNames = [],
        TotalGoalAmount = 0,
        TotalAmountAssigned = 0,
        TotalAmountSpent = 0,
        PercentageOfGoalsMet = 0,
        GoalTypes = [],
        AccountingPeriods = [],
    };

    private bool TryGetAccountingPeriodsInRange(
        Guid startAccountingPeriodId,
        Guid endAccountingPeriodId,
        out List<AccountingPeriod> accountingPeriods,
        out string? errorMessage)
    {
        accountingPeriods = [];
        errorMessage = null;

        if (!TryGetAccountingPeriod(startAccountingPeriodId, out AccountingPeriod? startAccountingPeriod))
        {
            errorMessage = $"Accounting Period with ID {startAccountingPeriodId} was not found.";
            return false;
        }

        if (!TryGetAccountingPeriod(endAccountingPeriodId, out AccountingPeriod? endAccountingPeriod))
        {
            errorMessage = $"Accounting Period with ID {endAccountingPeriodId} was not found.";
            return false;
        }

        if (startAccountingPeriod is null || endAccountingPeriod is null)
        {
            return false;
        }

        if (startAccountingPeriod.Year > endAccountingPeriod.Year ||
            (startAccountingPeriod.Year == endAccountingPeriod.Year && startAccountingPeriod.Month > endAccountingPeriod.Month))
        {
            errorMessage = "StartAccountingPeriodId must be earlier than or equal to EndAccountingPeriodId.";
            return false;
        }

        AccountingPeriod? currentAccountingPeriod = startAccountingPeriod;
        while (currentAccountingPeriod != null)
        {
            accountingPeriods.Add(currentAccountingPeriod);
            if (currentAccountingPeriod.Id == endAccountingPeriod.Id)
            {
                return true;
            }

            currentAccountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(currentAccountingPeriod.Id);
        }

        errorMessage = "The requested Accounting Period range must be contiguous.";
        return false;
    }

    private bool TryGetAccountingPeriod(Guid accountingPeriodId, out AccountingPeriod? accountingPeriod)
    {
        if (accountingPeriodConverter.TryToDomain(accountingPeriodId, out accountingPeriod))
        {
            return true;
        }

        accountingPeriod = null;
        return false;
    }

    private static List<Goal> SortGoals(IEnumerable<Goal> goals, GoalSortOrderModel? sort) => sort switch
    {
        GoalSortOrderModel.AccountingPeriod => goals.OrderBy(goal => goal.AccountingPeriodId.Value).ThenBy(goal => goal.Fund.Name).ToList(),
        GoalSortOrderModel.AccountingPeriodDescending => goals.OrderByDescending(goal => goal.AccountingPeriodId.Value).ThenByDescending(goal => goal.Fund.Name).ToList(),
        GoalSortOrderModel.Fund => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId.Value).ToList(),
        GoalSortOrderModel.FundDescending => goals.OrderByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId.Value).ToList(),
        GoalSortOrderModel.GoalAmount => goals.OrderBy(goal => goal.GoalAmount).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId.Value).ToList(),
        GoalSortOrderModel.GoalAmountDescending => goals.OrderByDescending(goal => goal.GoalAmount).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId.Value).ToList(),
        _ => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId.Value).ToList(),
    };

    private List<GoalDashboardGoalTypeSummaryModel> BuildTypeSummary(IEnumerable<Goal> goals) => goals
        .GroupBy(goal => goal.GoalType)
        .Select(group => BuildGoalTypeGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.GoalType)
        .ToList();

    private List<GoalDashboardAccountingPeriodSummaryModel> BuildAccountingPeriodSummary(IEnumerable<Goal> goals) => goals
        .GroupBy(goal => goal.AccountingPeriodId)
        .Select(group => BuildAccountingPeriodGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.AccountingPeriodName, StringComparer.OrdinalIgnoreCase)
        .ToList();

    private GoalDashboardGoalTypeSummaryModel BuildGoalTypeGroupSummary(List<Goal> goals, GoalType goalType)
    {
        decimal goalAmount = goals.Sum(goal => goal.GoalAmount);
        decimal amountAssigned = goals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        decimal amountSpent = goals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metGoals = goals.Count(goal => goal.IsAssignmentGoalMet && goal.IsSpendingGoalMet);

        return new GoalDashboardGoalTypeSummaryModel
        {
            GoalType = GoalTypeConverter.ToModel(goalType),
            GoalAmount = goalAmount,
            AmountAssigned = amountAssigned,
            AmountSpent = amountSpent,
            PercentageOfGoalsMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
        };
    }

    private GoalDashboardAccountingPeriodSummaryModel BuildAccountingPeriodGroupSummary(List<Goal> goals, AccountingPeriodId accountingPeriodId)
    {
        AccountingPeriod accountingPeriod = accountingPeriodRepository.GetById(accountingPeriodId);
        decimal goalAmount = goals.Sum(goal => goal.GoalAmount);
        decimal amountAssigned = goals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        decimal amountSpent = goals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metGoals = goals.Count(goal => goal.IsAssignmentGoalMet && goal.IsSpendingGoalMet);

        return new GoalDashboardAccountingPeriodSummaryModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            GoalAmount = goalAmount,
            AmountAssigned = amountAssigned,
            AmountSpent = amountSpent,
            PercentageOfGoalsMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
        };
    }

    private AccountingPeriodFundBalanceHistory GetFundBalanceHistory(Goal goal) =>
        accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(goal.AccountingPeriodId)
            .FundBalances
            .Single(fundBalance => fundBalance.Fund.Id == goal.Fund.Id);

    private static List<string> NormalizeNames(IEnumerable<string>? names) =>
        names?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
}
