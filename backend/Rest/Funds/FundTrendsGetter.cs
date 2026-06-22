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
/// Class that handles retrieving Fund trends data for a date range or Accounting Period range.
/// </summary>
public class FundTrendsGetter(
    IFundRepository fundRepository,
    IFundBalanceHistoryRepository fundBalanceHistoryRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Retrieves the Fund trends data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        FundTrendsQueryParameterModel request,
        out FundTrendsModel results,
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
            results = CreateEmptyResult(FundTrendsModeModel.AccountingPeriod);
            return false;
        }

        if (hasDateRangeParameters)
        {
            return TryGetDateMode(request, requestedFundNames, errors, out results);
        }

        return TryGetAccountingPeriodMode(request, requestedFundNames, errors, out results);
    }

    /// <summary>
    /// Retrieves the Fund trends data for a range of accounting periods.
    /// </summary>
    private bool TryGetAccountingPeriodMode(
        FundTrendsQueryParameterModel request,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out FundTrendsModel results)
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
            results = CreateEmptyResult(FundTrendsModeModel.AccountingPeriod);
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
            results = CreateEmptyResult(FundTrendsModeModel.AccountingPeriod);
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod>? accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult(FundTrendsModeModel.AccountingPeriod);
            return false;
        }
        List<FundTrendsRow> baseRows = BuildAccountingPeriodRows(accountingPeriods);
        List<string> availableFundNames = GetAvailableFundNames(baseRows);
        List<FundTrendsRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        List<FundTrendsBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (decimal totalAmountAssigned, decimal totalAmountSpent) = GetTransactionTotalsForAccountingPeriods(
            filteredRows.Select(row => row.Id).ToList(),
            accountingPeriods);
        results = new FundTrendsModel
        {
            Mode = FundTrendsModeModel.AccountingPeriod,
            Funds = new CollectionModel<FundTrendsFundModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<FundTrendsBalanceEventModel>
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
    /// Retrieves the Fund trends data for a range of dates.
    /// </summary>
    private bool TryGetDateMode(
        FundTrendsQueryParameterModel request,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out FundTrendsModel results)
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
            results = CreateEmptyResult(FundTrendsModeModel.Date);
            return false;
        }
        if (request.StartDate > request.EndDate)
        {
            errors.Add(nameof(request.StartDate), ["StartDate must be earlier than or equal to EndDate."]);
            results = CreateEmptyResult(FundTrendsModeModel.Date);
            return false;
        }
        var dates = new DateRange(request.StartDate.Value, request.EndDate.Value)
            .GetInclusiveDates()
            .ToList();
        List<FundTrendsRow> baseRows = BuildDateRows(dates);
        List<string> availableFundNames = GetAvailableFundNames(baseRows);
        List<FundTrendsRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableFundNames(requestedFundNames, availableFundNames));
        List<FundTrendsBalanceEventRow> balanceEvents = BuildBalanceEventsForDates(
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
        results = new FundTrendsModel
        {
            Mode = FundTrendsModeModel.Date,
            Funds = new CollectionModel<FundTrendsFundModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<FundTrendsBalanceEventModel>
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

    private static IEnumerable<FundTrendsRow> ApplyPaging(
        IEnumerable<FundTrendsRow> rows,
        FundTrendsQueryParameterModel request) => rows
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static IEnumerable<FundTrendsBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<FundTrendsBalanceEventRow> rows,
        FundTrendsQueryParameterModel request) => rows
        .Skip(request.BalanceEventOffset ?? 0)
        .Take(request.BalanceEventLimit ?? int.MaxValue);

    private static FundTrendsModel CreateEmptyResult(FundTrendsModeModel mode) => new()
    {
        Mode = mode,
        Funds = new CollectionModel<FundTrendsFundModel>
        {
            Items = [],
            TotalCount = 0,
        },
        BalanceEvents = new CollectionModel<FundTrendsBalanceEventModel>
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

    private List<FundTrendsRow> BuildAccountingPeriodRows(
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

        IEnumerable<FundTrendsRow> rows = fundsById.Values.Select(fund =>
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

            return new FundTrendsRow(
                fund.Id.Value,
                fund.Name,
                periodBalances.First().OpeningBalance,
                periodBalances.Last().ClosingBalance,
                periodBalances,
                null);
        });

        return rows.ToList();
    }

    private List<FundTrendsRow> BuildDateRows(
        IReadOnlyList<DateOnly> dates)
    {
        IEnumerable<Fund> funds = fundRepository.GetAll()
            .Where(fund => fund.IsOnboarded || fund.OnboardedBalance == null);

        IEnumerable<FundTrendsRow> rows = funds.Select(fund =>
        {
            var dateModels = dates.Select(date => new FundTrendsDateModel
            {
                Date = date,
                Balance = GetBalanceForDate(fund, date),
            }).ToList();

            return new FundTrendsRow(
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
        IReadOnlyCollection<FundTrendsRow> rows) => rows
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

    private static List<FundTrendsRow> ApplyFilters(
        IReadOnlyCollection<FundTrendsRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<FundTrendsRow> filteredRows = rows;

        if (fundNames != null)
        {
            filteredRows = filteredRows.Where(row => fundNames.Contains(row.Name));
        }

        return filteredRows.ToList();
    }

    private static List<FundTrendsBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<FundTrendsBalanceEventRow> rows,
        HashSet<string>? fundNames)
    {
        IEnumerable<FundTrendsBalanceEventRow> filteredRows = rows;

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

    private static List<FundTrendsRow> SortRows(
        List<FundTrendsRow> rows,
        FundTrendsSortOrderModel? sort) => sort switch
        {
            null or FundTrendsSortOrderModel.Name => rows.OrderBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.NameDescending => rows.OrderByDescending(row => row.Name).ToList(),
            FundTrendsSortOrderModel.OpeningBalance => rows.OrderBy(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.OpeningBalanceDescending => rows.OrderByDescending(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.ClosingBalance => rows.OrderBy(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.ClosingBalanceDescending => rows.OrderByDescending(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.NetChange => rows.OrderBy(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            FundTrendsSortOrderModel.NetChangeDescending => rows.OrderByDescending(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            _ => rows,
        };

    private static List<FundTrendsBalanceEventRow> SortBalanceEvents(
        List<FundTrendsBalanceEventRow> rows,
        FundTrendsBalanceEventSortOrderModel? sort) => sort switch
        {
            FundTrendsBalanceEventSortOrderModel.FundName => rows
                .OrderBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.FundNameDescending => rows
                .OrderByDescending(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or FundTrendsBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.FundId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.FundName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            FundTrendsBalanceEventSortOrderModel.AmountDescending => rows
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

    private List<FundTrendsBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
        IReadOnlyCollection<AccountingPeriod> accountingPeriods)
    {
        var accountingPeriodIds = accountingPeriods.Select(accountingPeriod => accountingPeriod.Id.Value).ToHashSet();
        return BuildBalanceEvents(
            transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId.Value));
    }

    private List<FundTrendsBalanceEventRow> BuildBalanceEventsForDates(
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
                    totalAmountAssigned += incomeTransaction.Destinations
                        .SelectMany(destination => destination.FundAssignments)
                        .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                        .Sum(fundAssignment => fundAssignment.Amount);
                }
                if (transaction is SpendingTransaction spendingTransaction)
                {
                    totalAmountSpent += spendingTransaction.Destinations
                        .SelectMany(destination => destination.FundAssignments)
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
            totalAmountAssigned += transaction.Destinations
                .SelectMany(destination => destination.FundAssignments)
                .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                .Sum(fundAssignment => fundAssignment.Amount);
        }
        foreach (SpendingTransaction transaction in transactionRepository.GetAllSpendingTransactionsByDateRange(startDate, endDate).OfType<SpendingTransaction>())
        {
            totalAmountSpent += transaction.Destinations
                .SelectMany(destination => destination.FundAssignments)
                .Where(fundAssignment => filteredFundIds.Contains(fundAssignment.FundId.Value))
                .Sum(fundAssignment => fundAssignment.Amount);
        }
        return (totalAmountAssigned, totalAmountSpent);
    }

    private List<FundTrendsBalanceEventRow> BuildBalanceEvents(
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

    private static IEnumerable<FundTrendsBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (FundTrendsBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    spendingTransaction.Destinations.SelectMany(destination => destination.FundAssignments).ToList(),
                    spendingTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    FundTrendsBalanceEventTypeModel.Debit))
                {
                    yield return balanceEvent;
                }

                break;
            case IncomeTransaction incomeTransaction:
                foreach (FundTrendsBalanceEventRow balanceEvent in BuildBalanceEventsByFundAssignments(
                    transaction,
                    incomeTransaction.Destinations.SelectMany(destination => destination.FundAssignments).ToList(),
                    incomeTransaction.Date,
                    fundsById,
                    accountingPeriodsById,
                    FundTrendsBalanceEventTypeModel.Credit))
                {
                    yield return balanceEvent;
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<FundTrendsBalanceEventRow> BuildBalanceEventsByFundAssignments(
        Transaction transaction,
        IReadOnlyCollection<FundAmount> fundAssignments,
        DateOnly date,
        IReadOnlyDictionary<Guid, Fund> fundsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        FundTrendsBalanceEventTypeModel type)
    {
        if (!accountingPeriodsById.TryGetValue(transaction.AccountingPeriodId.Value, out AccountingPeriod? accountingPeriod))
        {
            yield break;
        }

        foreach (FundAmount? fundAssignment in fundAssignments.Where(fa => fundsById.ContainsKey(fa.FundId.Value)))
        {
            Fund fund = fundsById[fundAssignment.FundId.Value];
            yield return new FundTrendsBalanceEventRow(
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

    private static List<FundTrendsPeriodSummaryModel> BuildPeriodSummaries(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlyCollection<FundTrendsRow> rows) => accountingPeriods.Select(accountingPeriod =>
        {
            List<(decimal OpeningBalance, decimal ClosingBalance, string fundName)> balances = rows.Select(row =>
            {
                AccountingPeriodBalanceValue periodModel = row.AccountingPeriods!
                    .Single(period => period.AccountingPeriodId == accountingPeriod.Id.Value);
                return (periodModel.OpeningBalance, periodModel.ClosingBalance, row.Name);
            }).ToList();
            return new FundTrendsPeriodSummaryModel
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

    private static List<FundTrendsDateSummaryModel> BuildDateSummaries(
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<FundTrendsRow> rows) => dates.Select(date =>
        {
            List<(string fundName, decimal balance)> balances = rows.Select(row =>
            {
                FundTrendsDateModel dateModel = row.Dates!.Single(item => item.Date == date);
                return (row.Name, dateModel.Balance);
            }).ToList();
            return new FundTrendsDateSummaryModel
            {
                Date = date,
                TotalBalance = balances.Sum(balance => balance.balance),
                AssignedBalance = balances.Where(balance => balance.fundName != Fund.UnassignedFundName).Sum(balance => balance.balance),
                UnassignedBalance = balances.Where(balance => balance.fundName == Fund.UnassignedFundName).Sum(balance => balance.balance)
            };
        }).ToList();

    private static FundTrendsFundModel ToModel(FundTrendsRow row) => new()
    {
        Id = row.Id,
        Name = row.Name,
        StartingBalance = row.OpeningBalance,
        EndingBalance = row.ClosingBalance,
    };

    private static FundTrendsBalanceEventModel ToModel(FundTrendsBalanceEventRow row) => new()
    {
        FundId = row.FundId,
        FundName = row.FundName,
        Date = row.Date,
        AccountingPeriodId = row.AccountingPeriodId,
        AccountingPeriodName = row.AccountingPeriodName,
        Type = row.Type,
        IsPosted = row.IsPosted,
        Amount = row.Amount,
        TransactionId = row.TransactionId,
    };

    private sealed record AccountingPeriodBalanceValue(
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        decimal OpeningBalance,
        decimal ClosingBalance);

    private sealed record FundTrendsRow(
        Guid Id,
        string Name,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyCollection<AccountingPeriodBalanceValue>? AccountingPeriods,
        IReadOnlyCollection<FundTrendsDateModel>? Dates);

    private sealed record FundTrendsBalanceEventRow(
        Guid FundId,
        string FundName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        FundTrendsBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}