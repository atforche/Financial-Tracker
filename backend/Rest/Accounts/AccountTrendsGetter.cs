using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Accounts;
using Models.Transactions;
using Rest.AccountingPeriods;

namespace Rest.Accounts;

/// <summary>
/// Class that handles retrieving Account trends data for a date range or Accounting Period range.
/// </summary>
public class AccountTrendsGetter(
    IAccountRepository accountRepository,
    IAccountBalanceHistoryRepository accountBalanceHistoryRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Retrieves the Account trends data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        AccountTrendsQueryParameterModel request,
        out AccountTrendsModel results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        HashSet<AccountType>? accountTypes = null;
        if (request.AccountType is { Count: > 0 } requestAccountTypes)
        {
            accountTypes = [];
            foreach (AccountTypeModel requestAccountType in requestAccountTypes)
            {
                if (!AccountTypeConverter.TryToDomain(requestAccountType, out AccountType? accountType))
                {
                    errors.Add(nameof(request.AccountType), [$"Unrecognized Account Type: {requestAccountType}"]);
                    continue;
                }
                _ = accountTypes.Add(accountType.Value);
            }
        }
        HashSet<string>? requestedAccountNames = null;
        if (request.AccountName is { Count: > 0 } requestAccountNames)
        {
            requestedAccountNames = requestAccountNames
                .Where(accountName => !string.IsNullOrWhiteSpace(accountName))
                .Select(accountName => accountName.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requestedAccountNames.Count == 0)
            {
                requestedAccountNames = null;
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
            results = CreateEmptyResult(AccountTrendsModeModel.AccountingPeriod);
            return false;
        }

        if (hasDateRangeParameters)
        {
            return TryGetDateMode(request, accountTypes, requestedAccountNames, errors, out results);
        }

        return TryGetAccountingPeriodMode(request, accountTypes, requestedAccountNames, errors, out results);
    }

    /// <summary>
    /// Retrieves the Account trends data for a range of accounting periods.
    /// </summary>
    private bool TryGetAccountingPeriodMode(
        AccountTrendsQueryParameterModel request,
        IReadOnlySet<AccountType>? accountTypes,
        IReadOnlySet<string>? requestedAccountNames,
        Dictionary<string, string[]> errors,
        out AccountTrendsModel results)
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
            results = CreateEmptyResult(AccountTrendsModeModel.AccountingPeriod);
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
            results = CreateEmptyResult(AccountTrendsModeModel.AccountingPeriod);
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod>? accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult(AccountTrendsModeModel.AccountingPeriod);
            return false;
        }
        List<AccountTrendsRow> baseRows = BuildAccountingPeriodRows(accountingPeriods, accountTypes);
        List<string> availableAccountNames = GetAvailableAccountNames(baseRows);
        List<AccountTrendsRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        List<AccountTrendsBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods, accountTypes);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (IncomeAmountModel totalIncome, decimal totalSpending) = GetTransactionTotalsForAccountingPeriods(
            filteredRows.Select(row => row.Id).ToList(),
            accountingPeriods);
        results = new AccountTrendsModel
        {
            Mode = AccountTrendsModeModel.AccountingPeriod,
            Accounts = new CollectionModel<AccountTrendsAccountModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<AccountTrendsBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(balanceEvents, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = balanceEvents.Count,
            },
            AvailableAccountNames = availableAccountNames,
            TotalIncome = totalIncome,
            TotalSpending = totalSpending,
            AccountingPeriods = BuildPeriodSummaries(accountingPeriods, filteredRows),
            Dates = null,
        };
        return true;
    }

    /// <summary>
    /// Retrieves the Account trends data for a range of dates.
    /// </summary>
    private bool TryGetDateMode(
        AccountTrendsQueryParameterModel request,
        IReadOnlySet<AccountType>? accountTypes,
        IReadOnlySet<string>? requestedAccountNames,
        Dictionary<string, string[]> errors,
        out AccountTrendsModel results)
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
            results = CreateEmptyResult(AccountTrendsModeModel.Date);
            return false;
        }
        if (request.StartDate > request.EndDate)
        {
            errors.Add(nameof(request.StartDate), ["StartDate must be earlier than or equal to EndDate."]);
            results = CreateEmptyResult(AccountTrendsModeModel.Date);
            return false;
        }
        var dates = new DateRange(request.StartDate.Value, request.EndDate.Value)
            .GetInclusiveDates()
            .ToList();
        List<AccountTrendsRow> baseRows = BuildDateRows(dates, request.EndDate.Value, accountTypes);
        List<string> availableAccountNames = GetAvailableAccountNames(baseRows);
        List<AccountTrendsRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        List<AccountTrendsBalanceEventRow> balanceEvents = BuildBalanceEventsForDates(
            request.StartDate.Value,
            request.EndDate.Value,
            accountTypes);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (IncomeAmountModel totalIncome, decimal totalSpending) = GetTransactionTotalsForDates(
            filteredRows.Select(row => row.Id).ToList(),
            request.StartDate.Value,
            request.EndDate.Value);
        results = new AccountTrendsModel
        {
            Mode = AccountTrendsModeModel.Date,
            Accounts = new CollectionModel<AccountTrendsAccountModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<AccountTrendsBalanceEventModel>
            {
                Items = ApplyBalanceEventPaging(balanceEvents, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = balanceEvents.Count,
            },
            AvailableAccountNames = availableAccountNames,
            TotalIncome = totalIncome,
            TotalSpending = totalSpending,
            AccountingPeriods = null,
            Dates = BuildDateSummaries(dates, filteredRows),
        };
        return true;
    }

    private static IEnumerable<AccountTrendsRow> ApplyPaging(
        IEnumerable<AccountTrendsRow> rows,
        AccountTrendsQueryParameterModel request) => rows
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static IEnumerable<AccountTrendsBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<AccountTrendsBalanceEventRow> rows,
        AccountTrendsQueryParameterModel request) => rows
        .Skip(request.BalanceEventOffset ?? 0)
        .Take(request.BalanceEventLimit ?? int.MaxValue);

    private static AccountTrendsModel CreateEmptyResult(AccountTrendsModeModel mode) => new()
    {
        Mode = mode,
        Accounts = new CollectionModel<AccountTrendsAccountModel>
        {
            Items = [],
            TotalCount = 0,
        },
        BalanceEvents = new CollectionModel<AccountTrendsBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AvailableAccountNames = [],
        TotalIncome = new IncomeAmountModel
        {
            Total = 0,
            Tracked = 0,
            Untracked = 0
        },
        TotalSpending = 0,
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

    private List<AccountTrendsRow> BuildAccountingPeriodRows(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlySet<AccountType>? accountTypes)
    {
        Dictionary<Guid, Account> accountsById = [];
        Dictionary<Guid, Dictionary<Guid, AccountingPeriodAccountBalanceHistory>> balanceHistoriesByAccountId = [];

        foreach (AccountingPeriod accountingPeriod in accountingPeriods)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            foreach (AccountingPeriodAccountBalanceHistory accountBalanceHistory in balanceHistory.AccountBalances)
            {
                if (accountTypes != null && !accountTypes.Contains(accountBalanceHistory.Account.Type))
                {
                    continue;
                }

                Guid accountId = accountBalanceHistory.Account.Id.Value;
                accountsById[accountId] = accountBalanceHistory.Account;
                if (!balanceHistoriesByAccountId.TryGetValue(accountId, out Dictionary<Guid, AccountingPeriodAccountBalanceHistory>? accountHistories))
                {
                    accountHistories = [];
                    balanceHistoriesByAccountId[accountId] = accountHistories;
                }

                accountHistories[accountingPeriod.Id.Value] = accountBalanceHistory;
            }
        }

        IEnumerable<AccountTrendsRow> rows = accountsById.Values.Select(account =>
        {
            var periodBalances = accountingPeriods.Select(accountingPeriod =>
            {
                AccountingPeriodAccountBalanceHistory? accountBalanceHistory = balanceHistoriesByAccountId[account.Id.Value]
                    .GetValueOrDefault(accountingPeriod.Id.Value);
                decimal openingBalance = accountBalanceHistory == null ? 0 : NormalizeBalance(account.Type, accountBalanceHistory.OpeningBalance);
                decimal closingBalance = accountBalanceHistory == null ? 0 : NormalizeBalance(account.Type, accountBalanceHistory.ClosingBalance);
                return new AccountingPeriodBalanceValue(
                    accountingPeriod.Id.Value,
                    accountingPeriod.Name,
                    openingBalance,
                    closingBalance);
            }).ToList();

            return new AccountTrendsRow(
                account.Id,
                account.Name,
                account.Type,
                periodBalances.First().OpeningBalance,
                periodBalances.Last().ClosingBalance,
                periodBalances,
                null);
        });

        return rows.ToList();
    }

    private List<AccountTrendsRow> BuildDateRows(
        IReadOnlyList<DateOnly> dates,
        DateOnly endDate,
        IReadOnlySet<AccountType>? accountTypes)
    {
        IEnumerable<Account> accounts = accountRepository.GetAll()
            .Where(account => accountTypes == null || accountTypes.Contains(account.Type))
            .Where(account => account.IsOnboarded || account.DateOpened == null || account.DateOpened <= endDate);

        IEnumerable<AccountTrendsRow> rows = accounts.Select(account =>
        {
            var dateModels = dates.Select(date => new AccountTrendsDateModel
            {
                Date = date,
                Balance = GetBalanceForDate(account, date),
            }).ToList();

            return new AccountTrendsRow(
                account.Id,
                account.Name,
                account.Type,
                dateModels.First().Balance,
                dateModels.Last().Balance,
                null,
                dateModels);
        });

        return rows.ToList();
    }

    private static List<string> GetAvailableAccountNames(
        IReadOnlyCollection<AccountTrendsRow> rows) => rows
        .Select(row => row.Name)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static HashSet<string>? GetApplicableAccountNames(
        IReadOnlySet<string>? requestedAccountNames,
        IReadOnlyCollection<string> availableAccountNames)
    {
        if (requestedAccountNames == null)
        {
            return null;
        }

        var applicableAccountNames = availableAccountNames
            .Where(requestedAccountNames.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return applicableAccountNames.Count == 0 ? null : applicableAccountNames;
    }

    private static List<AccountTrendsRow> ApplyFilters(
        IReadOnlyCollection<AccountTrendsRow> rows,
        HashSet<string>? accountNames)
    {
        IEnumerable<AccountTrendsRow> filteredRows = rows;

        if (accountNames != null)
        {
            filteredRows = filteredRows.Where(row => accountNames.Contains(row.Name));
        }

        return filteredRows.ToList();
    }

    private static List<AccountTrendsBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<AccountTrendsBalanceEventRow> rows,
        HashSet<string>? accountNames)
    {
        IEnumerable<AccountTrendsBalanceEventRow> filteredRows = rows;

        if (accountNames != null)
        {
            filteredRows = filteredRows.Where(row => accountNames.Contains(row.AccountName));
        }

        return filteredRows.ToList();
    }

    private decimal GetBalanceForDate(Account account, DateOnly date)
    {
        if (account.DateOpened is DateOnly dateOpened && date < dateOpened)
        {
            return 0;
        }
        AccountBalanceHistory? accountBalanceHistory = accountBalanceHistoryRepository
            .GetLatestHistoryEarlierThan(account.Id, date, int.MaxValue);
        decimal balance = accountBalanceHistory?.PostedBalance ?? account.OnboardedBalance ?? 0;
        return NormalizeBalance(account.Type, balance);
    }

    private static List<AccountTrendsRow> SortRows(
        List<AccountTrendsRow> rows,
        AccountTrendsSortOrderModel? sort) => sort switch
        {
            null or AccountTrendsSortOrderModel.Name => rows.OrderBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.NameDescending => rows.OrderByDescending(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.Type => rows.OrderBy(row => row.Type).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.TypeDescending => rows.OrderByDescending(row => row.Type).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.OpeningBalance => rows.OrderBy(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.OpeningBalanceDescending => rows.OrderByDescending(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.ClosingBalance => rows.OrderBy(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.ClosingBalanceDescending => rows.OrderByDescending(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.NetChange => rows.OrderBy(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountTrendsSortOrderModel.NetChangeDescending => rows.OrderByDescending(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            _ => rows,
        };

    private static List<AccountTrendsBalanceEventRow> SortBalanceEvents(
        List<AccountTrendsBalanceEventRow> rows,
        AccountTrendsBalanceEventSortOrderModel? sort) => sort switch
        {
            AccountTrendsBalanceEventSortOrderModel.AccountName => rows
                .OrderBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.AccountNameDescending => rows
                .OrderByDescending(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or AccountTrendsBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.AccountId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.AccountId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountTrendsBalanceEventSortOrderModel.AmountDescending => rows
                .OrderByDescending(row => row.Amount)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            _ => rows,
        };

    private List<AccountTrendsBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
        IReadOnlyCollection<AccountingPeriod> accountingPeriods,
        IReadOnlySet<AccountType>? accountTypes)
    {
        var accountingPeriodIds = accountingPeriods
            .Select(accountingPeriod => accountingPeriod.Id.Value)
            .ToHashSet();

        return BuildBalanceEvents(
            transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId.Value),
            accountTypes);
    }

    private List<AccountTrendsBalanceEventRow> BuildBalanceEventsForDates(
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlySet<AccountType>? accountTypes) => BuildBalanceEvents(
            transaction => true,
            accountTypes,
            effectiveDate => effectiveDate >= startDate && effectiveDate <= endDate);

    private (IncomeAmountModel TotalIncome, decimal TotalSpending) GetTransactionTotalsForAccountingPeriods(List<AccountId> filteredAccountIds, IReadOnlyList<AccountingPeriod> accountingPeriods)
    {
        decimal totalTrackedSpending = 0;
        decimal totalUntrackedSpending = 0;
        decimal totalSpending = 0;

        foreach (AccountingPeriodId accountingPeriodId in accountingPeriods.Select(accountingPeriod => accountingPeriod.Id).ToHashSet())
        {
            foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(accountingPeriodId))
            {
                if (transaction is IncomeTransaction incomeTransaction)
                {
                    totalTrackedSpending += incomeTransaction.Destinations
                        .Where(destination => filteredAccountIds.Contains(destination.Account.Id) && destination.Account.Type.IsTracked())
                        .Sum(destination => destination.Amount);
                    totalUntrackedSpending += incomeTransaction.Destinations
                        .Where(destination => filteredAccountIds.Contains(destination.Account.Id) && !destination.Account.Type.IsTracked())
                        .Sum(destination => destination.Amount);
                }
                if (transaction is SpendingTransaction spendingTransaction &&
                    spendingTransaction.Source.Account != null &&
                    filteredAccountIds.Contains(spendingTransaction.Source.Account.Id))
                {
                    totalSpending += transaction.Amount;
                }
            }
        }
        return (
            new IncomeAmountModel
            {
                Total = totalTrackedSpending + totalUntrackedSpending,
                Tracked = totalTrackedSpending,
                Untracked = totalUntrackedSpending
            },
            totalSpending);
    }

    private (IncomeAmountModel TotalIncome, decimal TotalSpending) GetTransactionTotalsForDates(List<AccountId> filteredAccountIds, DateOnly startDate, DateOnly endDate)
    {
        decimal totalTrackedIncome = 0;
        decimal totalUntrackedIncome = 0;
        decimal totalSpending = 0;

        foreach (IncomeTransaction transaction in transactionRepository.GetAllIncomeTransactionsByDateRange(startDate, endDate).OfType<IncomeTransaction>())
        {
            totalTrackedIncome += transaction.Destinations
                .Where(destination => filteredAccountIds.Contains(destination.Account.Id) && destination.Account.Type.IsTracked() && destination.PostedDate != null && destination.PostedDate >= startDate && destination.PostedDate <= endDate)
                .Sum(destination => destination.Amount);
            totalUntrackedIncome += transaction.Destinations
                .Where(destination => filteredAccountIds.Contains(destination.Account.Id) && !destination.Account.Type.IsTracked() && destination.PostedDate != null && destination.PostedDate >= startDate && destination.PostedDate <= endDate)
                .Sum(destination => destination.Amount);
        }
        foreach (SpendingTransaction transaction in transactionRepository.GetAllSpendingTransactionsByDateRange(startDate, endDate).OfType<SpendingTransaction>())
        {
            if (transaction.Source.Account != null && filteredAccountIds.Contains(transaction.Source.Account.Id))
            {
                totalSpending += transaction.Amount;
            }
        }
        return (
            new IncomeAmountModel
            {
                Total = totalTrackedIncome + totalUntrackedIncome,
                Tracked = totalTrackedIncome,
                Untracked = totalUntrackedIncome
            },
            totalSpending);
    }

    private List<AccountTrendsBalanceEventRow> BuildBalanceEvents(
        Func<Transaction, bool> transactionFilter,
        IReadOnlySet<AccountType>? accountTypes,
        Func<DateOnly, bool>? effectiveDateFilter = null)
    {
        var accountsById = accountRepository.GetAll()
            .ToDictionary(account => account.Id.Value);
        var accountingPeriodsById = accountingPeriodRepository.GetAll()
            .ToDictionary(accountingPeriod => accountingPeriod.Id.Value);

        return transactionRepository.GetAll()
            .Where(transactionFilter)
            .SelectMany(transaction => BuildBalanceEvents(transaction, accountsById, accountingPeriodsById, accountTypes))
            .Where(row => effectiveDateFilter == null || effectiveDateFilter(row.Date))
            .ToList();
    }

    private static IEnumerable<AccountTrendsBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Account> accountsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<AccountType>? accountTypes)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                if (spendingTransaction.Source.Account != null)
                {
                    foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        spendingTransaction.Source.Account.Id,
                        spendingTransaction.Source.PostedDate,
                        AccountTrendsBalanceEventTypeModel.Debit))
                    {
                        yield return balanceEvent;
                    }
                }
                foreach (SpendingTransactionDestination destination in spendingTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                            transaction,
                            accountsById,
                            accountingPeriodsById,
                            accountTypes,
                            destination.Account.Id,
                            destination.PostedDate,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.Amount))
                        {
                            yield return balanceEvent;
                        }
                    }
                }

                break;
            case IncomeTransaction incomeTransaction:
                if (incomeTransaction.Source.Account != null)
                {
                    foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        incomeTransaction.Source.Account.Id,
                        incomeTransaction.Source.PostedDate,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        transaction.Amount))
                    {
                        yield return balanceEvent;
                    }
                }
                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        destination.Account.Id,
                        destination.PostedDate,
                        AccountTrendsBalanceEventTypeModel.Credit,
                        destination.Amount))
                    {
                        yield return balanceEvent;
                    }
                }

                break;
            case AccountTransaction accountTransaction:
                if (accountTransaction.Source.Account != null)
                {
                    foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        accountTransaction.Source.Account.Id,
                        accountTransaction.Source.PostedDate,
                        AccountTrendsBalanceEventTypeModel.Debit))
                    {
                        yield return balanceEvent;
                    }
                }
                foreach (AccountTransactionDestination destination in accountTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        foreach (AccountTrendsBalanceEventRow balanceEvent in BuildBalanceEvents(
                            transaction,
                            accountsById,
                            accountingPeriodsById,
                            accountTypes,
                            destination.Account.Id,
                            destination.PostedDate,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.Amount))
                        {
                            yield return balanceEvent;
                        }
                    }
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<AccountTrendsBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Account> accountsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<AccountType>? accountTypes,
        AccountId? accountId,
        DateOnly? postedDate,
        AccountTrendsBalanceEventTypeModel type,
        decimal? amountOverride = null)
    {
        if (accountId == null)
        {
            yield break;
        }

        if (!accountsById.TryGetValue(accountId.Value, out Account? account))
        {
            yield break;
        }

        if (accountTypes != null && !accountTypes.Contains(account.Type))
        {
            yield break;
        }

        if (!accountingPeriodsById.TryGetValue(transaction.AccountingPeriodId.Value, out AccountingPeriod? accountingPeriod))
        {
            yield break;
        }

        yield return new AccountTrendsBalanceEventRow(
            account.Id,
            account.Name,
            postedDate ?? transaction.Date,
            transaction.AccountingPeriodId.Value,
            accountingPeriod.Name,
            type,
            postedDate != null,
            amountOverride ?? transaction.Amount,
            transaction.Date,
            transaction.Sequence,
            transaction.Id.Value);
    }

    private static List<AccountTrendsPeriodSummaryModel> BuildPeriodSummaries(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlyCollection<AccountTrendsRow> rows) => accountingPeriods.Select(accountingPeriod =>
        {
            List<(AccountType AccountType, decimal OpeningBalance, decimal ClosingBalance)> balances = rows
                .Select(row =>
                {
                    AccountingPeriodBalanceValue periodModel = row.AccountingPeriods!
                        .Single(period => period.AccountingPeriodId == accountingPeriod.Id.Value);
                    return (row.Type, periodModel.OpeningBalance, periodModel.ClosingBalance);
                })
                .ToList();

            decimal trackedOpeningBalance = balances
                .Where(balance => balance.AccountType.IsTracked())
                .Sum(balance => balance.OpeningBalance);
            decimal trackedClosingBalance = balances
                .Where(balance => balance.AccountType.IsTracked())
                .Sum(balance => balance.ClosingBalance);
            decimal untrackedOpeningBalance = balances
                .Where(balance => !balance.AccountType.IsTracked())
                .Sum(balance => balance.OpeningBalance);
            decimal untrackedClosingBalance = balances
                .Where(balance => !balance.AccountType.IsTracked())
                .Sum(balance => balance.ClosingBalance);

            return new AccountTrendsPeriodSummaryModel
            {
                AccountingPeriodId = accountingPeriod.Id.Value,
                AccountingPeriodName = accountingPeriod.Name,
                Year = accountingPeriod.Year,
                Month = accountingPeriod.Month,
                TotalOpeningBalance = balances.Sum(balance => balance.OpeningBalance),
                TotalClosingBalance = balances.Sum(balance => balance.ClosingBalance),
                TrackedOpeningBalance = trackedOpeningBalance,
                TrackedClosingBalance = trackedClosingBalance,
                UntrackedOpeningBalance = untrackedOpeningBalance,
                UntrackedClosingBalance = untrackedClosingBalance,
                OpeningBalanceByAccountType = balances
                    .GroupBy(balance => balance.AccountType)
                    .OrderBy(group => group.Key)
                    .Select(group => new AccountTypeBalanceModel
                    {
                        AccountType = AccountTypeConverter.ToModel(group.Key),
                        TotalBalance = group.Sum(balance => balance.OpeningBalance),
                    })
                    .ToList(),
                ClosingBalanceByAccountType = balances
                    .GroupBy(balance => balance.AccountType)
                    .OrderBy(group => group.Key)
                    .Select(group => new AccountTypeBalanceModel
                    {
                        AccountType = AccountTypeConverter.ToModel(group.Key),
                        TotalBalance = group.Sum(balance => balance.ClosingBalance),
                    })
                    .ToList(),
            };
        }).ToList();

    private static List<AccountTrendsDateSummaryModel> BuildDateSummaries(
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<AccountTrendsRow> rows) => dates.Select(date =>
        {
            List<(AccountType AccountType, decimal Balance)> balances = rows
                .Select(row =>
                {
                    AccountTrendsDateModel dateModel = row.Dates!
                        .Single(item => item.Date == date);
                    return (row.Type, dateModel.Balance);
                })
                .ToList();

            return new AccountTrendsDateSummaryModel
            {
                Date = date,
                TotalBalance = balances.Sum(balance => balance.Balance),
                TrackedBalance = balances.Where(balance => balance.AccountType.IsTracked()).Sum(balance => balance.Balance),
                UntrackedBalance = balances.Where(balance => !balance.AccountType.IsTracked()).Sum(balance => balance.Balance),
                BalanceByAccountType = balances
                    .GroupBy(balance => balance.AccountType)
                    .OrderBy(group => group.Key)
                    .Select(group => new AccountTypeBalanceModel
                    {
                        AccountType = AccountTypeConverter.ToModel(group.Key),
                        TotalBalance = group.Sum(balance => balance.Balance),
                    })
                    .ToList(),
            };
        }).ToList();

    private static AccountTrendsAccountModel ToModel(AccountTrendsRow row) => new()
    {
        Id = row.Id.Value,
        Name = row.Name,
        Type = AccountTypeConverter.ToModel(row.Type),
        StartingBalance = row.OpeningBalance,
        EndingBalance = row.ClosingBalance,
    };

    private static AccountTrendsBalanceEventModel ToModel(AccountTrendsBalanceEventRow row) => new()
    {
        AccountId = row.AccountId.Value,
        AccountName = row.AccountName,
        Date = row.Date,
        AccountingPeriodId = row.AccountingPeriodId,
        AccountingPeriodName = row.AccountingPeriodName,
        Type = row.Type,
        IsPosted = row.IsPosted,
        Amount = row.Amount,
        TransactionId = row.TransactionId,
    };

    private static decimal NormalizeBalance(AccountType accountType, decimal balance) =>
        accountType.IsDebt() ? -balance : balance;

    private sealed record AccountingPeriodBalanceValue(
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        decimal OpeningBalance,
        decimal ClosingBalance);

    private sealed record AccountTrendsRow(
        AccountId Id,
        string Name,
        AccountType Type,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyCollection<AccountingPeriodBalanceValue>? AccountingPeriods,
        IReadOnlyCollection<AccountTrendsDateModel>? Dates);

    private sealed record AccountTrendsBalanceEventRow(
        AccountId AccountId,
        string AccountName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        AccountTrendsBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}