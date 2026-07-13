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
/// Class that handles retrieving Goal trends data for an Accounting Period range.
/// </summary>
public class GoalTrendsGetter(
    AccountingPeriodConverter accountingPeriodConverter,
    AssignmentGoalRepository assignmentGoalRepository,
    GoalConverter goalConverter,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    SpendingGoalRepository spendingGoalRepository)
{
    /// <summary>
    /// Retrieves the Goal trends data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        GoalTrendsQueryParameterModel request,
        out GoalTrendsModel results,
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
        List<GoalTrendsBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods, requestedFundNames);
        balanceEvents = ApplyBalanceEventFilters(balanceEvents, requestedFundNames);

        var assignmentGoalModels = sortedAssignmentGoals
            .Select(goalConverter.ToModel)
            .ToList();
        var spendingGoalModels = sortedSpendingGoals
            .Select(goalConverter.ToModel)
            .ToList();

        List<GoalTrendsBalanceEventRow> assignmentBalanceEvents = SortBalanceEvents(
            balanceEvents.Where(row => row.Type == GoalTrendsBalanceEventType.Assignment).ToList(),
            request.AssignmentBalanceEventSort);
        List<GoalTrendsBalanceEventRow> spendingBalanceEvents = SortBalanceEvents(
            balanceEvents.Where(row => row.Type == GoalTrendsBalanceEventType.Spending).ToList(),
            request.SpendingBalanceEventSort);

        decimal totalAmountToAssign = sortedAssignmentGoals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = sortedAssignmentGoals.Sum(goal => GetGoalBalanceHistory(goal).AmountAssigned);
        int metAssignmentGoals = sortedAssignmentGoals.Count(goal => goal.IsGoalMet);
        decimal totalAmountToSpend = sortedSpendingGoals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = sortedSpendingGoals.Sum(goal => GetGoalBalanceHistory(goal).AmountSpent);
        int metSpendingGoals = sortedSpendingGoals.Count(goal => goal.IsGoalMet);

        List<GoalTrendsAssignmentGoalTypeSummaryModel> assignmentGoalTypeSummary = BuildAssignmentTypeSummary(sortedAssignmentGoals);
        List<GoalTrendsSpendingGoalTypeSummaryModel> spendingGoalTypeSummary = BuildSpendingTypeSummary(sortedSpendingGoals);
        List<GoalTrendsAccountingPeriodSummaryModel> accountingPeriodSummary = BuildAccountingPeriodSummary(
            accountingPeriods,
            sortedAssignmentGoals,
            sortedSpendingGoals);

        results = new GoalTrendsModel
        {
            AssignmentGoals = new CollectionModel<AssignmentGoalModel>
            {
                Items = ApplyGoalPaging(assignmentGoalModels, request.AssignmentGoalOffset, request.AssignmentGoalLimit),
                TotalCount = assignmentGoalModels.Count,
            },
            AssignmentBalanceEvents = new CollectionModel<GoalTrendsBalanceEventModel>
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
            SpendingBalanceEvents = new CollectionModel<GoalTrendsBalanceEventModel>
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
            PercentageOfAssignmentGoalsMet = new GoalPercentageMetModel
            {
                TotalCount = sortedAssignmentGoals.Count,
                MetCount = metAssignmentGoals,
                PercentageMet = sortedAssignmentGoals.Count == 0 ? 0 : metAssignmentGoals * 100m / sortedAssignmentGoals.Count,
            },
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfSpendingGoalsMet = new GoalPercentageMetModel
            {
                TotalCount = sortedSpendingGoals.Count,
                MetCount = metSpendingGoals,
                PercentageMet = sortedSpendingGoals.Count == 0 ? 0 : metSpendingGoals * 100m / sortedSpendingGoals.Count,
            },
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

    private static IEnumerable<GoalTrendsBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<GoalTrendsBalanceEventRow> rows,
        int? offset,
        int? limit) => rows
        .Skip(offset ?? 0)
        .Take(limit ?? int.MaxValue);

    private static List<GoalTrendsBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<GoalTrendsBalanceEventRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<GoalTrendsBalanceEventRow> filteredRows = rows;

        if (fundNames != null)
        {
            filteredRows = filteredRows.Where(row => fundNames.Contains(row.FundName));
        }

        return filteredRows.ToList();
    }

    private static List<GoalTrendsBalanceEventRow> SortBalanceEvents(
        List<GoalTrendsBalanceEventRow> rows,
        GoalTrendsBalanceEventSortOrderModel? sort) => sort switch
        {
            GoalTrendsBalanceEventSortOrderModel.FundName => rows
                .OrderBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.FundNameDescending => rows
                .OrderByDescending(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or GoalTrendsBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            GoalTrendsBalanceEventSortOrderModel.AmountDescending => rows
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

    private List<GoalTrendsBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
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

    private List<GoalTrendsBalanceEventRow> BuildBalanceEvents(
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

    private static IEnumerable<GoalTrendsBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<string>? requestedFundNames)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                var spendingAssignments = spendingTransaction.Destinations
                    .SelectMany(destination => destination.FundAssignments)
                    .GroupBy(fa => fa.FundId)
                    .Select(group => new FundAmount
                    {
                        FundId = group.Key,
                        Amount = group.Sum(fa => fa.Amount)
                    })
                    .ToList();
                foreach (GoalTrendsBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    spendingAssignments,
                    spendingTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    requestedFundNames,
                    GoalTrendsBalanceEventType.Spending))
                {
                    yield return balanceEvent;
                }

                break;
            case IncomeTransaction incomeTransaction:
                var incomeAssignments = incomeTransaction.Destinations
                    .SelectMany(destination => destination.FundAssignments)
                    .GroupBy(fa => fa.FundId)
                    .Select(group => new FundAmount
                    {
                        FundId = group.Key,
                        Amount = group.Sum(fa => fa.Amount)
                    })
                    .ToList();
                foreach (GoalTrendsBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    incomeAssignments,
                    incomeTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    requestedFundNames,
                    GoalTrendsBalanceEventType.Assignment))
                {
                    yield return balanceEvent;
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<GoalTrendsBalanceEventRow> BuildBalanceEventsByFundAssignments(
        Transaction transaction,
        IReadOnlyCollection<FundAmount> fundAssignments,
        DateOnly date,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<string>? requestedFundNames,
        GoalTrendsBalanceEventType type)
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

            yield return new GoalTrendsBalanceEventRow(
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

    private static GoalTrendsModel CreateEmptyResult() => new()
    {
        AssignmentGoals = new CollectionModel<AssignmentGoalModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AssignmentBalanceEvents = new CollectionModel<GoalTrendsBalanceEventModel>
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
        SpendingBalanceEvents = new CollectionModel<GoalTrendsBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        SpendingGoalTypes = [],
        AvailableFundNames = [],
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

    private List<GoalTrendsAssignmentGoalTypeSummaryModel> BuildAssignmentTypeSummary(IEnumerable<AssignmentGoal> goals) => goals
        .GroupBy(goal => goal.AssignmentGoalType)
        .Select(group => BuildAssignmentGoalTypeGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.AssignmentGoalType)
        .ToList();

    private List<GoalTrendsSpendingGoalTypeSummaryModel> BuildSpendingTypeSummary(IEnumerable<SpendingGoal> goals) => goals
        .GroupBy(goal => goal.SpendingGoalType)
        .Select(group => BuildSpendingGoalTypeGroupSummary(group.ToList(), group.Key))
        .OrderBy(summary => summary.SpendingGoalType)
        .ToList();

    private List<GoalTrendsAccountingPeriodSummaryModel> BuildAccountingPeriodSummary(
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

    private GoalTrendsAssignmentGoalTypeSummaryModel BuildAssignmentGoalTypeGroupSummary(
        List<AssignmentGoal> goals,
        AssignmentGoalType assignmentGoalType)
    {
        decimal totalAmountToAssign = goals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = goals.Sum(goal => GetGoalBalanceHistory(goal).AmountAssigned);
        int metGoals = goals.Count(goal => goal.IsGoalMet);

        return new GoalTrendsAssignmentGoalTypeSummaryModel
        {
            AssignmentGoalType = GoalTypeConverter.ToModel(assignmentGoalType),
            TotalAmountToAssign = totalAmountToAssign,
            TotalAmountAssigned = totalAmountAssigned,
            PercentageOfGoalsMet = new GoalPercentageMetModel
            {
                MetCount = metGoals,
                TotalCount = goals.Count,
                PercentageMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
            },
        };
    }

    private GoalTrendsSpendingGoalTypeSummaryModel BuildSpendingGoalTypeGroupSummary(
        List<SpendingGoal> goals,
        SpendingGoalType spendingGoalType)
    {
        decimal totalAmountToSpend = goals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = goals.Sum(goal => GetGoalBalanceHistory(goal).AmountSpent);
        int metGoals = goals.Count(goal => goal.IsGoalMet);

        return new GoalTrendsSpendingGoalTypeSummaryModel
        {
            SpendingGoalType = GoalTypeConverter.ToModel(spendingGoalType),
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfGoalsMet = new GoalPercentageMetModel
            {
                MetCount = metGoals,
                TotalCount = goals.Count,
                PercentageMet = goals.Count == 0 ? 0 : metGoals * 100m / goals.Count,
            },
        };
    }

    private GoalTrendsAccountingPeriodSummaryModel BuildAccountingPeriodGroupSummary(
        AccountingPeriod accountingPeriod,
        List<AssignmentGoal> assignmentGoals,
        List<SpendingGoal> spendingGoals)
    {
        decimal totalAmountToAssign = assignmentGoals.Sum(goal => goal.TotalAmountToAssign);
        decimal totalAmountAssigned = assignmentGoals.Sum(goal => GetGoalBalanceHistory(goal).AmountAssigned);
        int metAssignmentGoals = assignmentGoals.Count(goal => goal.IsGoalMet);
        decimal totalAmountToSpend = spendingGoals.Sum(goal => goal.TotalAmountToSpend);
        decimal totalAmountSpent = spendingGoals.Sum(goal => GetGoalBalanceHistory(goal).AmountSpent);
        int metSpendingGoals = spendingGoals.Count(goal => goal.IsGoalMet);

        return new GoalTrendsAccountingPeriodSummaryModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            TotalAmountToAssign = totalAmountToAssign,
            TotalAmountAssigned = totalAmountAssigned,
            PercentageOfAssignmentGoalsMet = new GoalPercentageMetModel
            {
                MetCount = metAssignmentGoals,
                TotalCount = assignmentGoals.Count,
                PercentageMet = assignmentGoals.Count == 0 ? 0 : metAssignmentGoals * 100m / assignmentGoals.Count,
            },
            TotalAmountToSpend = totalAmountToSpend,
            TotalAmountSpent = totalAmountSpent,
            PercentageOfSpendingGoalsMet = new GoalPercentageMetModel
            {
                MetCount = metSpendingGoals,
                TotalCount = spendingGoals.Count,
                PercentageMet = spendingGoals.Count == 0 ? 0 : metSpendingGoals * 100m / spendingGoals.Count,
            },
        };
    }

    private AccountingPeriodGoalBalanceHistory GetGoalBalanceHistory(AssignmentGoal goal) =>
        accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(goal.AccountingPeriodId ?? throw new InvalidOperationException("Assignment Goal must belong to an accounting period."))
            .GoalBalances
            .Single(goalBalance => goalBalance.Fund.Id == goal.Fund.Id);

    private AccountingPeriodGoalBalanceHistory GetGoalBalanceHistory(SpendingGoal goal) =>
        accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(goal.AccountingPeriodId ?? throw new InvalidOperationException("Spending Goal must belong to an accounting period."))
            .GoalBalances
            .Single(goalBalance => goalBalance.Fund.Id == goal.Fund.Id);

    private static GoalTrendsBalanceEventModel ToModel(GoalTrendsBalanceEventRow row) => new()
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

    private enum GoalTrendsBalanceEventType
    {
        Assignment,
        Spending,
    }

    private sealed record GoalTrendsBalanceEventRow(
        Guid FundId,
        string FundName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        GoalTrendsBalanceEventType Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}