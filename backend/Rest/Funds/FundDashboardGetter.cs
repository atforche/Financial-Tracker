using Domain;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Funds;
using Rest.AccountingPeriods;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving Fund dashboard data for a date range or Accounting Period range.
/// </summary>
public class FundDashboardGetter(
    IFundRepository fundRepository,
    IFundBalanceHistoryRepository fundBalanceHistoryRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Retrieves the Fund dashboard data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        FundDashboardQueryParameterModel request,
        out FundDashboardModel results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        HashSet<string>? requestedFundNames = null;
        if (request.FundName is { Count: > 0 } requestFundNames)
        {
            requestedFundNames = requestFundNames
                .Where(fundName => !string.IsNullOrWhiteSpace(fundName))
                .Select(fundName => fundName.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requestedFundNames.Count == 0)
            {
                requestedFundNames = null;
            }
        }

        bool hasDateRangeParameters = request.StartDate != null || request.EndDate != null;
        bool hasAccountingPeriodRangeParameters =
            request.StartAccountingPeriodId != null || request.EndAccountingPeriodId != null;
        if (hasDateRangeParameters == hasAccountingPeriodRangeParameters)
        {
            const string message = "Provide either a date range or an Accounting Period range.";
            errors.Add(nameof(request.StartDate), [message]);
            errors.Add(nameof(request.StartAccountingPeriodId), [message]);
            results = CreateEmptyResult(FundDashboardModeModel.AccountingPeriod);
            return false;
        }

        if (hasDateRangeParameters)
        {
            return TryGetDateMode(request, requestedFundNames, errors, out results);
        }

        return TryGetAccountingPeriodMode(request, requestedFundNames, errors, out results);
    }

    /// <summary>
    /// Retrieves the Fund dashboard data for a range of accounting periods.
    /// </summary>
    private bool TryGetAccountingPeriodMode(
        FundDashboardQueryParameterModel request,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out FundDashboardModel results)
    {
        AccountingPeriod? startAccountingPeriod = null;
        if (request.StartAccountingPeriodId is null)
        {
            errors.Add(nameof(request.StartAccountingPeriodId), ["StartAccountingPeriodId is required."]);
        }
        else if (!accountingPeriodConverter.TryToDomain(request.StartAccountingPeriodId.Value, out startAccountingPeriod))
        {
            errors.Add(
                nameof(request.StartAccountingPeriodId),
                [$"Accounting Period with ID {request.StartAccountingPeriodId.Value} was not found."]);
        }
        AccountingPeriod? endAccountingPeriod = null;
        if (request.EndAccountingPeriodId is null)
        {
            errors.Add(nameof(request.EndAccountingPeriodId), ["EndAccountingPeriodId is required."]);
        }
        else if (!accountingPeriodConverter.TryToDomain(request.EndAccountingPeriodId.Value, out endAccountingPeriod))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                [$"Accounting Period with ID {request.EndAccountingPeriodId.Value} was not found."]);
        }
        if (errors.Count > 0 || startAccountingPeriod == null || endAccountingPeriod == null)
        {
            results = CreateEmptyResult(FundDashboardModeModel.AccountingPeriod);
            return false;
        }
        bool startAccountingPeriodIsAfterEndAccountingPeriod =
            startAccountingPeriod.Year > endAccountingPeriod.Year ||
            (startAccountingPeriod.Year == endAccountingPeriod.Year &&
             startAccountingPeriod.Month > endAccountingPeriod.Month);
        if (startAccountingPeriodIsAfterEndAccountingPeriod)
        {
            errors.Add(
                nameof(request.StartAccountingPeriodId),
                ["StartAccountingPeriodId must be earlier than or equal to EndAccountingPeriodId."]);
            results = CreateEmptyResult(FundDashboardModeModel.AccountingPeriod);
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod>? accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult(FundDashboardModeModel.AccountingPeriod);
            return false;
        }
        List<FundDashboardRow> baseRows = BuildAccountingPeriodRows(accountingPeriods);
        List<string> availableFundNames = GetAvailableFundNames(baseRows);
        List<FundDashboardRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        List<FundDashboardBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (decimal totalAmountAssigned, decimal totalAmountSpent) = GetTransactionTotalsForAccountingPeriods(
            filteredRows.Select(row => row.Id).ToList(),
            accountingPeriods);
        results = new FundDashboardModel
        {
            Mode = FundDashboardModeModel.AccountingPeriod,
            Funds = new CollectionModel<FundDashboardFundModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<FundDashboardBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(balanceEvents, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = balanceEvents.Count,
            },
            AvailableFundNames = availableFundNames,
            TotalAmountAssigned = totalAmountAssigned,
            TotalAmountSpent = totalAmountSpent,
            AccountingPeriods = BuildPeriodSummaries(accountingPeriods, filteredRows),
            Dates = null,
        };
        return true;
    }

    /// <summary>
    /// Retrieves the Fund dashboard data for a range of dates.
    /// </summary>
    private bool TryGetDateMode(
        FundDashboardQueryParameterModel request,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out FundDashboardModel results)
    {
        if (request.StartDate is null)
        {
            errors.Add(nameof(request.StartDate), ["StartDate is required."]);
        }
        if (request.EndDate is null)
        {
            errors.Add(nameof(request.EndDate), ["EndDate is required."]);
        }
        if (errors.Count > 0 || request.StartDate == null || request.EndDate == null)
        {
            results = CreateEmptyResult(FundDashboardModeModel.Date);
            return false;
        }
        if (request.StartDate > request.EndDate)
        {
            errors.Add(nameof(request.StartDate), ["StartDate must be earlier than or equal to EndDate."]);
            results = CreateEmptyResult(FundDashboardModeModel.Date);
            return false;
        }
        var dates = new DateRange(request.StartDate.Value, request.EndDate.Value)
            .GetInclusiveDates()
            .ToList();
        List<FundDashboardRow> baseRows = BuildDateRows(dates);
        List<string> availableFundNames = GetAvailableFundNames(baseRows);
        List<FundDashboardRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        List<FundDashboardBalanceEventRow> balanceEvents = BuildBalanceEventsForDates(
            request.StartDate.Value,
            request.EndDate.Value);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (decimal totalAmountAssigned, decimal totalAmountSpent) = GetTransactionTotalsForDates(
            filteredRows.Select(row => row.Id).ToList(),
            request.StartDate.Value,
            request.EndDate.Value);
        results = new FundDashboardModel
        {
            Mode = FundDashboardModeModel.Date,
            Funds = new CollectionModel<FundDashboardFundModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<FundDashboardBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(balanceEvents, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = balanceEvents.Count,
            },
            AvailableFundNames = availableFundNames,
            TotalAmountAssigned = totalAmountAssigned,
            TotalAmountSpent = totalAmountSpent,
            AccountingPeriods = null,
            Dates = BuildDateSummaries(dates, filteredRows),
        };
        return true;
    }

    private static IEnumerable<FundDashboardRow> ApplyPaging(
        IEnumerable<FundDashboardRow> rows,
        FundDashboardQueryParameterModel request) => rows
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static IEnumerable<FundDashboardBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<FundDashboardBalanceEventRow> rows,
        FundDashboardQueryParameterModel request) => rows
        .Skip(request.BalanceEventOffset ?? 0)
        .Take(request.BalanceEventLimit ?? int.MaxValue);

    private static FundDashboardModel CreateEmptyResult(FundDashboardModeModel mode) => new()
    {
        Mode = mode,
        Funds = new CollectionModel<FundDashboardFundModel>
        {
            Items = [],
            TotalCount = 0,
        },
        BalanceEvents = new CollectionModel<FundDashboardBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AvailableFundNames = [],
        TotalAmountAssigned = 0,
        TotalAmountSpent = 0,
        AccountingPeriods = null,
        Dates = null,
    };

    private bool TryGetAccountingPeriodsInRange(
        AccountingPeriod startAccountingPeriod,
        AccountingPeriod endAccountingPeriod,
        out List<AccountingPeriod> accountingPeriods)
    {
        accountingPeriods = [];

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

        accountingPeriods = [];
        return false;
    }

    private List<FundDashboardRow> BuildAccountingPeriodRows(
        IReadOnlyList<AccountingPeriod> accountingPeriods)
    {
        Dictionary<Guid, Fund> fundsById = [];
        Dictionary<Guid, Dictionary<Guid, AccountingPeriodFundBalanceHistory>> balanceHistoriesByFundId = [];

        foreach (AccountingPeriod accountingPeriod in accountingPeriods)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in balanceHistory.FundBalances)
            {
                Guid fundId = fundBalanceHistory.Fund.Id.Value;
                fundsById[fundId] = fundBalanceHistory.Fund;
                if (!balanceHistoriesByFundId.TryGetValue(fundId, out Dictionary<Guid, AccountingPeriodFundBalanceHistory>? fundHistories))
                {
                    fundHistories = [];
                    balanceHistoriesByFundId[fundId] = fundHistories;
                }

                fundHistories[accountingPeriod.Id.Value] = fundBalanceHistory;
            }
        }

        IEnumerable<FundDashboardRow> rows = fundsById.Values.Select(fund =>
        {
            var periodBalances = accountingPeriods.Select(accountingPeriod =>
            {
                AccountingPeriodFundBalanceHistory? fundBalanceHistory = balanceHistoriesByFundId[fund.Id.Value]
                    .GetValueOrDefault(accountingPeriod.Id.Value);
                decimal openingBalance = fundBalanceHistory == null ? 0 : fundBalanceHistory.OpeningBalance;
                decimal closingBalance = fundBalanceHistory == null ? 0 : fundBalanceHistory.ClosingBalance;
                return new AccountingPeriodBalanceValue(
                    accountingPeriod.Id.Value,
                    accountingPeriod.Name,
                    openingBalance,
                    closingBalance);
            }).ToList();

            return new FundDashboardRow(
                fund.Id.Value,
                fund.Name,
                periodBalances.First().OpeningBalance,
                periodBalances.Last().ClosingBalance,
                periodBalances,
                null);
        });

        return rows.ToList();
    }

    private List<FundDashboardRow> BuildDateRows(
        IReadOnlyList<DateOnly> dates)
    {
        IEnumerable<Fund> funds = fundRepository.GetAll()
            .Where(fund => fund.IsOnboarded || fund.OnboardedBalance == null);

        IEnumerable<FundDashboardRow> rows = funds.Select(fund =>
        {
            var dateModels = dates.Select(date => new FundDashboardDateModel
            {
                Date = date,
                Balance = GetBalanceForDate(fund, date),
            }).ToList();

            return new FundDashboardRow(
                fund.Id.Value,
                fund.Name,
                dateModels.First().Balance,
                dateModels.Last().Balance,
                null,
                dateModels);
        });

        return rows.ToList();
    }

    private static List<string> GetAvailableFundNames(
        IReadOnlyCollection<FundDashboardRow> rows) => rows
        .Select(row => row.Name)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static HashSet<string>? GetApplicableFundNames(
        IReadOnlySet<string>? requestedFundNames,
        IReadOnlyCollection<string> availableFundNames)
    {
        if (requestedFundNames == null)
        {
            return null;
        }

        var applicableFundNames = availableFundNames
            .Where(requestedFundNames.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return applicableFundNames.Count == 0 ? null : applicableFundNames;
    }

    private static List<FundDashboardRow> ApplyFilters(
        IReadOnlyCollection<FundDashboardRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<FundDashboardRow> filteredRows = rows;

        if (fundNames != null)
        {
            filteredRows = filteredRows.Where(row => fundNames.Contains(row.Name));
        }

        return filteredRows.ToList();
    }

    private static List<FundDashboardBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<FundDashboardBalanceEventRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<FundDashboardBalanceEventRow> filteredRows = rows;

        if (fundNames != null)
        {
            filteredRows = filteredRows.Where(row => fundNames.Contains(row.FundName));
        }

        return filteredRows.ToList();
    }

    private decimal GetBalanceForDate(Fund fund, DateOnly date)
    {
        FundBalanceHistory? fundBalanceHistory = fundBalanceHistoryRepository
            .GetLatestHistoryEarlierThan(fund.Id, date, int.MaxValue);
        decimal balance = fundBalanceHistory?.PostedBalance ?? fund.OnboardedBalance ?? 0;
        return balance;
    }

    private static List<FundDashboardRow> SortRows(
        List<FundDashboardRow> rows,
        FundDashboardSortOrderModel? sort) => sort switch
        {
            null or FundDashboardSortOrderModel.Name => rows.OrderBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.NameDescending => rows.OrderByDescending(row => row.Name).ToList(),
            FundDashboardSortOrderModel.OpeningBalance => rows.OrderBy(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.OpeningBalanceDescending => rows.OrderByDescending(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.ClosingBalance => rows.OrderBy(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.ClosingBalanceDescending => rows.OrderByDescending(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.NetChange => rows.OrderBy(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundDashboardSortOrderModel.NetChangeDescending => rows.OrderByDescending(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            _ => rows,
        };

    private static List<FundDashboardBalanceEventRow> SortBalanceEvents(
        List<FundDashboardBalanceEventRow> rows,
        FundDashboardBalanceEventSortOrderModel? sort) => sort switch
        {
            FundDashboardBalanceEventSortOrderModel.FundName => rows
                .OrderBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.FundNameDescending => rows
                .OrderByDescending(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or FundDashboardBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundDashboardBalanceEventSortOrderModel.AmountDescending => rows
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

    private List<FundDashboardBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
        IReadOnlyCollection<AccountingPeriod> accountingPeriods)
    {
        var accountingPeriodIds = accountingPeriods.Select(accountingPeriod => accountingPeriod.Id.Value).ToHashSet();
        return BuildBalanceEvents(
            transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId.Value));
    }

    private List<FundDashboardBalanceEventRow> BuildBalanceEventsForDates(
        DateOnly startDate,
        DateOnly endDate) => BuildBalanceEvents(
            transaction => true,
            effectiveDate => effectiveDate >= startDate && effectiveDate <= endDate);

    private (decimal TotalAmountAssigned, decimal TotalAmountSpent) GetTransactionTotalsForAccountingPeriods(
        List<Guid> filteredFundIds,
        IReadOnlyList<AccountingPeriod> accountingPeriods)
    {
        decimal totalAmountAssigned = 0;
        decimal totalAmountSpent = 0;

        foreach (AccountingPeriodId accountingPeriodId in accountingPeriods.Select(accountingPeriod => accountingPeriod.Id).ToHashSet())
        {
            foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(accountingPeriodId))
            {
                if (transaction is IncomeTransaction incomeTransaction)
                {
                    totalAmountAssigned += incomeTransaction.FundAssignments
                        .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                        .Sum(fundAssignment => fundAssignment.Amount);
                }
                if (transaction is SpendingTransaction spendingTransaction)
                {
                    totalAmountSpent += spendingTransaction.FundAssignments
                        .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                        .Sum(fundAssignment => fundAssignment.Amount);
                }
            }
        }
        return (totalAmountAssigned, totalAmountSpent);
    }

    private (decimal TotalAmountAssigned, decimal TotalAmountSpent) GetTransactionTotalsForDates(
        List<Guid> filteredFundIds,
        DateOnly startDate,
        DateOnly endDate)
    {
        decimal totalAmountAssigned = 0;
        decimal totalAmountSpent = 0;

        foreach (IncomeTransaction transaction in transactionRepository.GetAllIncomeTransactionsByDateRange(startDate, endDate).OfType<IncomeTransaction>())
        {
            totalAmountAssigned += transaction.FundAssignments
                .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                .Sum(fundAssignment => fundAssignment.Amount);
        }
        foreach (SpendingTransaction transaction in transactionRepository.GetAllSpendingTransactionsByDateRange(startDate, endDate).OfType<SpendingTransaction>())
        {
            totalAmountSpent += transaction.FundAssignments
                .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                .Sum(fundAssignment => fundAssignment.Amount);
        }
        return (totalAmountAssigned, totalAmountSpent);
    }

    private List<FundDashboardBalanceEventRow> BuildBalanceEvents(
        Func<Transaction, bool> transactionFilter,
        Func<DateOnly, bool>? effectiveDateFilter = null)
    {
        var fundsById = fundRepository.GetAll().ToDictionary(fund => fund.Id.Value);
        var accountingPeriodsById = accountingPeriodRepository.GetAll().ToDictionary(accountingPeriod => accountingPeriod.Id.Value);
        return transactionRepository.GetAll()
            .Where(transactionFilter)
            .SelectMany(transaction => BuildBalanceEvents(transaction, fundsById, accountingPeriodsById))
            .Where(row => effectiveDateFilter == null || effectiveDateFilter(row.Date))
            .ToList();
    }

    private static IEnumerable<FundDashboardBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (FundDashboardBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    spendingTransaction.FundAssignments,
                    spendingTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    FundDashboardBalanceEventTypeModel.Debit))
                {
                    yield return balanceEvent;
                }

                break;
            case IncomeTransaction incomeTransaction:
                foreach (FundDashboardBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    incomeTransaction.FundAssignments,
                    incomeTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    FundDashboardBalanceEventTypeModel.Credit))
                {
                    yield return balanceEvent;
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<FundDashboardBalanceEventRow> BuildBalanceEventsByFundAssignments(
        Transaction transaction,
        IReadOnlyCollection<FundAmount> fundAssignments,
        DateOnly date,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        FundDashboardBalanceEventTypeModel type)
    {
        if (!accountingPeriodsById.TryGetValue(transaction.AccountingPeriodId.Value, out AccountingPeriod? accountingPeriod))
        {
            yield break;
        }

        foreach (FundAmount? fundAssignment in fundAssignments.Where(fa => fundsById.ContainsKey(fa.FundId.Value)))
        {
            Fund fund = fundsById[fundAssignment.FundId.Value];
            yield return new FundDashboardBalanceEventRow(
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

    private static List<FundDashboardPeriodSummaryModel> BuildPeriodSummaries(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlyCollection<FundDashboardRow> rows) => accountingPeriods.Select(accountingPeriod =>
        {
            List<(decimal OpeningBalance, decimal ClosingBalance, string fundName)> balances = rows.Select(row =>
            {
                AccountingPeriodBalanceValue periodModel = row.AccountingPeriods!
                    .Single(period => period.AccountingPeriodId == accountingPeriod.Id.Value);
                return (periodModel.OpeningBalance, periodModel.ClosingBalance, row.Name);
            }).ToList();
            return new FundDashboardPeriodSummaryModel
            {
                AccountingPeriodId = accountingPeriod.Id.Value,
                AccountingPeriodName = accountingPeriod.Name,
                Year = accountingPeriod.Year,
                Month = accountingPeriod.Month,
                TotalOpeningBalance = balances.Sum(balance => balance.OpeningBalance),
                TotalClosingBalance = balances.Sum(balance => balance.ClosingBalance),
                AssignedOpeningBalance = balances.Where(balance => balance.fundName != Fund.UnassignedFundName).Sum(balance => balance.OpeningBalance),
                AssignedClosingBalance = balances.Where(balance => balance.fundName != Fund.UnassignedFundName).Sum(balance => balance.ClosingBalance),
                UnassignedOpeningBalance = balances.Where(balance => balance.fundName == Fund.UnassignedFundName).Sum(balance => balance.OpeningBalance),
                UnassignedClosingBalance = balances.Where(balance => balance.fundName == Fund.UnassignedFundName).Sum(balance => balance.ClosingBalance),
            };
        }).ToList();

    private static List<FundDashboardDateSummaryModel> BuildDateSummaries(
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<FundDashboardRow> rows) => dates.Select(date =>
        {
            List<(string fundName, decimal balance)> balances = rows.Select(row =>
            {
                FundDashboardDateModel dateModel = row.Dates!.Single(item => item.Date == date);
                return (row.Name, dateModel.Balance);
            }).ToList();
            return new FundDashboardDateSummaryModel
            {
                Date = date,
                TotalBalance = balances.Sum(balance => balance.balance),
                AssignedBalance = balances.Where(balance => balance.fundName != Fund.UnassignedFundName).Sum(balance => balance.balance),
                UnassignedBalance = balances.Where(balance => balance.fundName == Fund.UnassignedFundName).Sum(balance => balance.balance)
            };
        }).ToList();

    private static FundDashboardFundModel ToModel(FundDashboardRow row) => new()
    {
        Id = row.Id,
        Name = row.Name,
        StartingBalance = row.OpeningBalance,
        EndingBalance = row.ClosingBalance,
    };

    private static FundDashboardBalanceEventModel ToModel(FundDashboardBalanceEventRow row) => new()
    {
        FundId = row.FundId,
        FundName = row.FundName,
        Date = row.Date,
        AccountingPeriodId = row.AccountingPeriodId,
        AccountingPeriodName = row.AccountingPeriodName,
        Type = row.Type,
        IsPosted = row.IsPosted,
        Amount = row.Amount,
    };

    private sealed record AccountingPeriodBalanceValue(
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        decimal OpeningBalance,
        decimal ClosingBalance);

    private sealed record FundDashboardRow(
        Guid Id,
        string Name,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyCollection<AccountingPeriodBalanceValue>? AccountingPeriods,
        IReadOnlyCollection<FundDashboardDateModel>? Dates);

    private sealed record FundDashboardBalanceEventRow(
        Guid FundId,
        string FundName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        FundDashboardBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}