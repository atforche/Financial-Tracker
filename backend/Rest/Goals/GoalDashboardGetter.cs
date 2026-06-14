using Data.Goals;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Goals;
using Rest.AccountingPeriods;

namespace Rest.Goals;

/// <summary>
/// Class that handles retrieving Goal dashboard data for an Accounting Period range.
/// </summary>
public class GoalDashboardGetter(
    AccountingPeriodConverter accountingPeriodConverter,
    AssignmentGoalRepository assignmentGoalRepository,
    GoalConverter goalConverter,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    SpendingGoalRepository spendingGoalRepository)
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

        if (request.StartAccountingPeriodId is null || request.EndAccountingPeriodId is null)
        {
            results = CreateEmptyResult();
            return true;
        }
        if (!TryGetAccountingPeriodsInRange(request.StartAccountingPeriodId.Value, request.EndAccountingPeriodId.Value, out List<AccountingPeriod> accountingPeriods, out string? rangeError))
        {
            errors.Add(nameof(request.EndAccountingPeriodId), [rangeError ?? "The requested Accounting Period range is invalid."]);
            results = CreateEmptyResult();
            return false;
        }
        HashSet<AssignmentGoalType>? requestedAssignmentGoalTypes = null;
        if (request.AssignmentGoalType is { Count: > 0 } requestedAssignmentGoalTypesInput)
        {
            requestedAssignmentGoalTypes = [];
            foreach (AssignmentGoalTypeModel goalTypeModel in requestedAssignmentGoalTypesInput)
            {
                if (!GoalTypeConverter.TryToDomain(goalTypeModel, out AssignmentGoalType? goalType) || goalType is null)
                {
                    errors.Add(nameof(request.AssignmentGoalType), [$"Unrecognized Assignment Goal Type: {goalTypeModel}"]);
                    continue;
                }

                _ = requestedAssignmentGoalTypes.Add(goalType.Value);
            }
        }

        HashSet<SpendingGoalType>? requestedSpendingGoalTypes = null;
        if (request.SpendingGoalType is { Count: > 0 } requestedSpendingGoalTypesInput)
        {
            requestedSpendingGoalTypes = [];
            foreach (SpendingGoalTypeModel goalTypeModel in requestedSpendingGoalTypesInput)
            {
                if (!GoalTypeConverter.TryToDomain(goalTypeModel, out SpendingGoalType? goalType) || goalType is null)
                {
                    errors.Add(nameof(request.SpendingGoalType), [$"Unrecognized Spending Goal Type: {goalTypeModel}"]);
                    continue;
                }

                _ = requestedSpendingGoalTypes.Add(goalType.Value);
            }
        }

        var requestedFundNames = NormalizeNames(request.FundName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var accountingPeriodIds = accountingPeriods
            .Select(accountingPeriod => accountingPeriod.Id)
            .ToHashSet();

        var baseAssignmentGoals = assignmentGoalRepository.GetAll()
            .Where(goal => goal.AccountingPeriodId is not null && accountingPeriodIds.Contains(goal.AccountingPeriodId))
            .ToList();
        var baseSpendingGoals = spendingGoalRepository.GetAll()
            .Where(goal => goal.AccountingPeriodId is not null && accountingPeriodIds.Contains(goal.AccountingPeriodId))
            .ToList();

        var availableFundNames = baseAssignmentGoals
            .Select(goal => goal.Fund.Name)
            .Concat(baseSpendingGoals.Select(goal => goal.Fund.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (errors.Count > 0)
        {
            results = CreateEmptyResult();
            return false;
        }

        var assignmentGoals = baseAssignmentGoals
            .Where(goal => requestedFundNames.Count == 0 || requestedFundNames.Contains(goal.Fund.Name))
            .Where(goal => requestedAssignmentGoalTypes == null || requestedAssignmentGoalTypes.Count == 0 || requestedAssignmentGoalTypes.Contains(goal.AssignmentGoalType))
            .ToList();
        var spendingGoals = baseSpendingGoals
            .Where(goal => requestedFundNames.Count == 0 || requestedFundNames.Contains(goal.Fund.Name))
            .Where(goal => requestedSpendingGoalTypes == null || requestedSpendingGoalTypes.Count == 0 || requestedSpendingGoalTypes.Contains(goal.SpendingGoalType))
            .ToList();

        List<AssignmentGoal> sortedAssignmentGoals = SortAssignmentGoals(assignmentGoals, request.AssignmentSort);
        List<SpendingGoal> sortedSpendingGoals = SortSpendingGoals(spendingGoals, request.SpendingSort);
        List<GoalDashboardBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods, requestedFundNames);
        balanceEvents = ApplyBalanceEventFilters(balanceEvents, requestedFundNames);

        var assignmentGoalModels = sortedAssignmentGoals
            .Select(goalConverter.ToModel)
            .ToList();
        var spendingGoalModels = sortedSpendingGoals
            .Select(goalConverter.ToModel)
            .ToList();

        List<GoalDashboardBalanceEventRow> assignmentBalanceEvents = SortBalanceEvents(
            balanceEvents.Where(row => row.Type == GoalDashboardBalanceEventType.Assignment).ToList(),
            request.AssignmentBalanceEventSort);
        List<GoalDashboardBalanceEventRow> spendingBalanceEvents = SortBalanceEvents(
            balanceEvents.Where(row => row.Type == GoalDashboardBalanceEventType.Spending).ToList(),
            request.SpendingBalanceEventSort);

        decimal totalAmountToAssign = sortedAssignmentGoals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = sortedAssignmentGoals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        int metAssignmentGoals = sortedAssignmentGoals.Count(goal => goal.IsGoalMet);
        decimal totalAmountToSpend = sortedSpendingGoals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = sortedSpendingGoals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metSpendingGoals = sortedSpendingGoals.Count(goal => goal.IsGoalMet);

        List<GoalDashboardAssignmentGoalTypeSummaryModel> assignmentGoalTypeSummary = BuildAssignmentTypeSummary(sortedAssignmentGoals);
        List<GoalDashboardSpendingGoalTypeSummaryModel> spendingGoalTypeSummary = BuildSpendingTypeSummary(sortedSpendingGoals);
        List<GoalDashboardAccountingPeriodSummaryModel> accountingPeriodSummary = BuildAccountingPeriodSummary(
            accountingPeriods,
            sortedAssignmentGoals,
            sortedSpendingGoals);

        results = new GoalDashboardModel
        {
            AssignmentGoals = new CollectionModel<AssignmentGoalModel>
            {
                Items = ApplyGoalPaging(assignmentGoalModels, request.AssignmentGoalOffset, request.AssignmentGoalLimit),
                TotalCount = assignmentGoalModels.Count,
            },
            AssignmentBalanceEvents = new CollectionModel<GoalDashboardBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(
                        assignmentBalanceEvents,
                        request.AssignmentBalanceEventOffset,
                        request.AssignmentBalanceEventLimit)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = assignmentBalanceEvents.Count,
            },
            AssignmentGoalTypes = assignmentGoalTypeSummary,
            SpendingGoals = new CollectionModel<SpendingGoalModel>
            {
                Items = ApplyGoalPaging(spendingGoalModels, request.SpendingGoalOffset, request.SpendingGoalLimit),
                TotalCount = spendingGoalModels.Count,
            },
            SpendingBalanceEvents = new CollectionModel<GoalDashboardBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(
                        spendingBalanceEvents,
                        request.SpendingBalanceEventOffset,
                        request.SpendingBalanceEventLimit)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = spendingBalanceEvents.Count,
            },
            SpendingGoalTypes = spendingGoalTypeSummary,
            AvailableFundNames = availableFundNames,
            TotalAmountToAssign = totalAmountToAssign,
            TotalAmountAssigned = totalAmountAssigned,
            PercentageOfAssignmentGoalsMet = sortedAssignmentGoals.Count == 0 ? 0 : metAssignmentGoals * 100m / sortedAssignmentGoals.Count,
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfSpendingGoalsMet = sortedSpendingGoals.Count == 0 ? 0 : metSpendingGoals * 100m / sortedSpendingGoals.Count,
            AccountingPeriods = accountingPeriodSummary,
        };

        return true;
    }

    private static List<T> ApplyGoalPaging<T>(
        IEnumerable<T> items,
        int? offset,
        int? limit) => items
        .Skip(offset ?? 0)
        .Take(limit ?? int.MaxValue)
        .ToList();

    private static IEnumerable<GoalDashboardBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<GoalDashboardBalanceEventRow> rows,
        int? offset,
        int? limit) => rows
        .Skip(offset ?? 0)
        .Take(limit ?? int.MaxValue);

    private static List<GoalDashboardBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<GoalDashboardBalanceEventRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<GoalDashboardBalanceEventRow> filteredRows = rows;

        if (fundNames != null)
        {
            filteredRows = filteredRows.Where(row => fundNames.Contains(row.FundName));
        }

        return filteredRows.ToList();
    }

    private static List<GoalDashboardBalanceEventRow> SortBalanceEvents(
        List<GoalDashboardBalanceEventRow> rows,
        GoalDashboardBalanceEventSortOrderModel? sort) => sort switch
        {
            GoalDashboardBalanceEventSortOrderModel.FundName => rows
                .OrderBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.FundNameDescending => rows
                .OrderByDescending(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or GoalDashboardBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalDashboardBalanceEventSortOrderModel.AmountDescending => rows
                .OrderByDescending(row => row.Amount)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            _ => rows,
        };

    private List<GoalDashboardBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        IReadOnlySet<string>? requestedFundNames)
    {
        var accountingPeriodIds = accountingPeriods
            .Select(accountingPeriod => accountingPeriod.Id.Value)
            .ToHashSet();

        return BuildBalanceEvents(
            transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId.Value),
            requestedFundNames);
    }

    private List<GoalDashboardBalanceEventRow> BuildBalanceEvents(
        Func<Transaction, bool> transactionFilter,
        IReadOnlySet<string>? requestedFundNames)
    {
        var fundsById = assignmentGoalRepository.GetAll()
            .Select(goal => goal.Fund)
            .Concat(spendingGoalRepository.GetAll().Select(goal => goal.Fund))
            .Distinct()
            .ToDictionary(fund => fund.Id.Value);
        var accountingPeriodsById = accountingPeriodRepository.GetAll().ToDictionary(period => period.Id.Value);

        return transactionRepository.GetAll()
            .Where(transactionFilter)
            .SelectMany(transaction => BuildBalanceEvents(transaction, fundsById, accountingPeriodsById, requestedFundNames))
            .ToList();
    }

    private static IEnumerable<GoalDashboardBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<string>? requestedFundNames)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (GoalDashboardBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    spendingTransaction.FundAssignments,
                    spendingTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    requestedFundNames,
                    GoalDashboardBalanceEventType.Spending))
                {
                    yield return balanceEvent;
                }

                break;
            case IncomeTransaction incomeTransaction:
                foreach (GoalDashboardBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    incomeTransaction.FundAssignments,
                    incomeTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    requestedFundNames,
                    GoalDashboardBalanceEventType.Assignment))
                {
                    yield return balanceEvent;
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<GoalDashboardBalanceEventRow> BuildBalanceEventsByFundAssignments(
        Transaction transaction,
        IReadOnlyCollection<FundAmount> fundAssignments,
        DateOnly date,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<string>? requestedFundNames,
        GoalDashboardBalanceEventType type)
    {
        if (!accountingPeriodsById.TryGetValue(transaction.AccountingPeriodId.Value, out AccountingPeriod? accountingPeriod))
        {
            yield break;
        }

        foreach (FundAmount? fundAssignment in fundAssignments.Where(fa => fundsById.ContainsKey(fa.FundId.Value)))
        {
            Fund fund = fundsById[fundAssignment.FundId.Value];
            if (requestedFundNames != null && !requestedFundNames.Contains(fund.Name))
            {
                continue;
            }

            yield return new GoalDashboardBalanceEventRow(
                fund.Id.Value,
                fund.Name,
                date,
                transaction.AccountingPeriodId.Value,
                accountingPeriod.Name,
                type,
                true,
                fundAssignment.Amount,
                transaction.Date,
                transaction.Sequence,
                transaction.Id.Value);
        }
    }

    private static GoalDashboardModel CreateEmptyResult() => new()
    {
        AssignmentGoals = new CollectionModel<AssignmentGoalModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AssignmentBalanceEvents = new CollectionModel<GoalDashboardBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AssignmentGoalTypes = [],
        SpendingGoals = new CollectionModel<SpendingGoalModel>
        {
            Items = [],
            TotalCount = 0,
        },
        SpendingBalanceEvents = new CollectionModel<GoalDashboardBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        SpendingGoalTypes = [],
        AvailableFundNames = [],
        TotalAmountToAssign = 0,
        TotalAmountAssigned = 0,
        PercentageOfAssignmentGoalsMet = 0,
        TotalAmountToSpend = 0,
        TotalAmountSpent = 0,
        PercentageOfSpendingGoalsMet = 0,
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

    private static List<AssignmentGoal> SortAssignmentGoals(
        IEnumerable<AssignmentGoal> goals,
        AssignmentGoalSortOrderModel? sort) => sort switch
        {
            AssignmentGoalSortOrderModel.AccountingPeriod => goals.OrderBy(goal => goal.AccountingPeriodId).ThenBy(goal => goal.Fund.Name).ToList(),
            AssignmentGoalSortOrderModel.AccountingPeriodDescending => goals.OrderByDescending(goal => goal.AccountingPeriodId).ThenByDescending(goal => goal.Fund.Name).ToList(),
            null or AssignmentGoalSortOrderModel.Fund => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.FundDescending => goals.OrderByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.Type => goals.OrderBy(goal => goal.AssignmentGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.TypeDescending => goals.OrderByDescending(goal => goal.AssignmentGoalType).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.GoalAmount => goals.OrderBy(goal => goal.GoalAmount).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.GoalAmountDescending => goals.OrderByDescending(goal => goal.GoalAmount).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.TotalAmountToAssign => goals.OrderBy(goal => goal.TotalAmountToAssign).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.TotalAmountToAssignDescending => goals.OrderByDescending(goal => goal.TotalAmountToAssign).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.TotalAmountAssigned => goals.OrderBy(goal => goal.TotalAmountAssigned).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.TotalAmountAssignedDescending => goals.OrderByDescending(goal => goal.TotalAmountAssigned).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.IsMet => goals.OrderBy(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            AssignmentGoalSortOrderModel.IsMetDescending => goals.OrderByDescending(goal => goal.IsGoalMet).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            _ => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
        };

    private static List<SpendingGoal> SortSpendingGoals(
        IEnumerable<SpendingGoal> goals,
        SpendingGoalSortOrderModel? sort) => sort switch
        {
            SpendingGoalSortOrderModel.AccountingPeriod => goals.OrderBy(goal => goal.AccountingPeriodId).ThenBy(goal => goal.Fund.Name).ToList(),
            SpendingGoalSortOrderModel.AccountingPeriodDescending => goals.OrderByDescending(goal => goal.AccountingPeriodId).ThenByDescending(goal => goal.Fund.Name).ToList(),
            null or SpendingGoalSortOrderModel.Fund => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.FundDescending => goals.OrderByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.Type => goals.OrderBy(goal => goal.SpendingGoalType).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.TypeDescending => goals.OrderByDescending(goal => goal.SpendingGoalType).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.TotalAmountToSpend => goals.OrderBy(goal => goal.TotalAmountToSpend).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.TotalAmountToSpendDescending => goals.OrderByDescending(goal => goal.TotalAmountToSpend).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.TotalAmountSpent => goals.OrderBy(goal => goal.TotalAmountSpent).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.TotalAmountSpentDescending => goals.OrderByDescending(goal => goal.TotalAmountSpent).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.IsMet => goals.OrderBy(goal => goal.IsGoalMet).ThenBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
            SpendingGoalSortOrderModel.IsMetDescending => goals.OrderByDescending(goal => goal.IsGoalMet).ThenByDescending(goal => goal.Fund.Name).ThenByDescending(goal => goal.AccountingPeriodId).ToList(),
            _ => goals.OrderBy(goal => goal.Fund.Name).ThenBy(goal => goal.AccountingPeriodId).ToList(),
        };

    private List<GoalDashboardAssignmentGoalTypeSummaryModel> BuildAssignmentTypeSummary(IEnumerable<AssignmentGoal> goals) => goals
        .GroupBy(goal => goal.AssignmentGoalType)
        .Select(group => BuildAssignmentGoalTypeGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.AssignmentGoalType)
        .ToList();

    private List<GoalDashboardSpendingGoalTypeSummaryModel> BuildSpendingTypeSummary(IEnumerable<SpendingGoal> goals) => goals
        .GroupBy(goal => goal.SpendingGoalType)
        .Select(group => BuildSpendingGoalTypeGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.SpendingGoalType)
        .ToList();

    private List<GoalDashboardAccountingPeriodSummaryModel> BuildAccountingPeriodSummary(
        IEnumerable<AccountingPeriod> accountingPeriods,
        IEnumerable<AssignmentGoal> assignmentGoals,
        IEnumerable<SpendingGoal> spendingGoals)
    {
        var assignmentGoalsByPeriodId = assignmentGoals
            .Where(goal => goal.AccountingPeriodId is not null)
            .GroupBy(goal => goal.AccountingPeriodId!)
            .ToDictionary(group => group.Key, group => group.ToList());
        var spendingGoalsByPeriodId = spendingGoals
            .Where(goal => goal.AccountingPeriodId is not null)
            .GroupBy(goal => goal.AccountingPeriodId!)
            .ToDictionary(group => group.Key, group => group.ToList());

        return accountingPeriods
            .Select(accountingPeriod => BuildAccountingPeriodGroupSummary(
                accountingPeriod,
                assignmentGoalsByPeriodId.GetValueOrDefault(accountingPeriod.Id, []),
                spendingGoalsByPeriodId.GetValueOrDefault(accountingPeriod.Id, [])))
            .ToList();
    }

    private GoalDashboardAssignmentGoalTypeSummaryModel BuildAssignmentGoalTypeGroupSummary(
        List<AssignmentGoal> goals,
        AssignmentGoalType assignmentGoalType)
    {
        decimal totalAmountToAssign = goals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = goals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        int metGoals = goals.Count(goal => goal.IsGoalMet);

        return new GoalDashboardAssignmentGoalTypeSummaryModel
        {
            AssignmentGoalType = GoalTypeConverter.ToModel(assignmentGoalType),
            TotalAmountToAssign = totalAmountToAssign,
            TotalAmountAssigned = totalAmountAssigned,
            PercentageOfGoalsMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
        };
    }

    private GoalDashboardSpendingGoalTypeSummaryModel BuildSpendingGoalTypeGroupSummary(
        List<SpendingGoal> goals,
        SpendingGoalType spendingGoalType)
    {
        decimal totalAmountToSpend = goals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = goals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metGoals = goals.Count(goal => goal.IsGoalMet);

        return new GoalDashboardSpendingGoalTypeSummaryModel
        {
            SpendingGoalType = GoalTypeConverter.ToModel(spendingGoalType),
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfGoalsMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
        };
    }

    private GoalDashboardAccountingPeriodSummaryModel BuildAccountingPeriodGroupSummary(
        AccountingPeriod accountingPeriod,
        List<AssignmentGoal> assignmentGoals,
        List<SpendingGoal> spendingGoals)
    {
        decimal totalAmountToAssign = assignmentGoals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = assignmentGoals.Sum(goal => GetFundBalanceHistory(goal).AmountAssigned);
        int metAssignmentGoals = assignmentGoals.Count(goal => goal.IsGoalMet);
        decimal totalAmountToSpend = spendingGoals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = spendingGoals.Sum(goal => GetFundBalanceHistory(goal).AmountSpent);
        int metSpendingGoals = spendingGoals.Count(goal => goal.IsGoalMet);

        return new GoalDashboardAccountingPeriodSummaryModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            TotalAmountToAssign = totalAmountToAssign,
            TotalAmountAssigned = totalAmountAssigned,
            PercentageOfAssignmentGoalsMet = assignmentGoals.Count == 0 ? 0 : metAssignmentGoals * 100m / assignmentGoals.Count,
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfSpendingGoalsMet = spendingGoals.Count == 0 ? 0 : metSpendingGoals * 100m / spendingGoals.Count,
        };
    }

    private AccountingPeriodFundBalanceHistory GetFundBalanceHistory(AssignmentGoal goal) =>
        accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(goal.AccountingPeriodId ?? throw new InvalidOperationException("Assignment Goal must belong to an accounting period."))
            .FundBalances
            .Single(fundBalance => fundBalance.Fund.Id == goal.Fund.Id);

    private AccountingPeriodFundBalanceHistory GetFundBalanceHistory(SpendingGoal goal) =>
        accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(goal.AccountingPeriodId ?? throw new InvalidOperationException("Spending Goal must belong to an accounting period."))
            .FundBalances
            .Single(fundBalance => fundBalance.Fund.Id == goal.Fund.Id);

    private static GoalDashboardBalanceEventModel ToModel(GoalDashboardBalanceEventRow row) => new()
    {
        FundId = row.FundId,
        FundName = row.FundName,
        Date = row.Date,
        AccountingPeriodId = row.AccountingPeriodId,
        AccountingPeriodName = row.AccountingPeriodName,
        IsPosted = row.IsPosted,
        Amount = row.Amount,
        TransactionId = row.TransactionId,
    };

    private static List<string> NormalizeNames(IEnumerable<string>? names) =>
        names?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

    private enum GoalDashboardBalanceEventType
    {
        Assignment,
        Spending,
    }

    private sealed record GoalDashboardBalanceEventRow(
        Guid FundId,
        string FundName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        GoalDashboardBalanceEventType Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}