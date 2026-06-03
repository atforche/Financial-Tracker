using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.Accounts;
using Rest.AccountingPeriods;

namespace Rest.Accounts;

/// <summary>
/// Class that handles retrieving Account dashboard data for a date range or Accounting Period range.
/// </summary>
public class AccountDashboardGetter(
    IAccountRepository accountRepository,
    IAccountBalanceHistoryRepository accountBalanceHistoryRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter)
{
    /// <summary>
    /// Retrieves the Account dashboard data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        AccountDashboardQueryParameterModel request,
        out AccountDashboardModel results,
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
            results = CreateEmptyResult(AccountDashboardModeModel.AccountingPeriod);
            return false;
        }

        if (hasDateRangeParameters)
        {
            return TryGetDateMode(request, accountTypes, requestedAccountNames, errors, out results);
        }

        return TryGetAccountingPeriodMode(request, accountTypes, requestedAccountNames, errors, out results);
    }

    /// <summary>
    /// Retrieves the Account dashboard data for a range of accounting periods.
    /// </summary>
    private bool TryGetAccountingPeriodMode(
        AccountDashboardQueryParameterModel request,
        IReadOnlySet<AccountType>? accountTypes,
        IReadOnlySet<string>? requestedAccountNames,
        Dictionary<string, string[]> errors,
        out AccountDashboardModel results)
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
            results = CreateEmptyResult(AccountDashboardModeModel.AccountingPeriod);
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
            results = CreateEmptyResult(AccountDashboardModeModel.AccountingPeriod);
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod>? accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult(AccountDashboardModeModel.AccountingPeriod);
            return false;
        }
        List<AccountDashboardRow> baseRows = BuildAccountingPeriodRows(accountingPeriods, accountTypes);
        List<string> availableAccountNames = GetAvailableAccountNames(baseRows);
        List<AccountDashboardRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        List<AccountDashboardBalanceEventRow> balanceEvents = BuildBalanceEventsForAccountingPeriods(accountingPeriods, accountTypes);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (decimal totalIncome, decimal totalSpending) = GetTransactionTotalsForAccountingPeriods(
            filteredRows.Select(row => row.Id).ToList(),
            accountingPeriods);
        results = new AccountDashboardModel
        {
            Mode = AccountDashboardModeModel.AccountingPeriod,
            Accounts = new CollectionModel<AccountDashboardAccountModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<AccountDashboardBalanceEventModel>
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
    /// Retrieves the Account dashboard data for a range of dates.
    /// </summary>
    private bool TryGetDateMode(
        AccountDashboardQueryParameterModel request,
        IReadOnlySet<AccountType>? accountTypes,
        IReadOnlySet<string>? requestedAccountNames,
        Dictionary<string, string[]> errors,
        out AccountDashboardModel results)
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
            results = CreateEmptyResult(AccountDashboardModeModel.Date);
            return false;
        }
        if (request.StartDate > request.EndDate)
        {
            errors.Add(nameof(request.StartDate), ["StartDate must be earlier than or equal to EndDate."]);
            results = CreateEmptyResult(AccountDashboardModeModel.Date);
            return false;
        }
        var dates = new DateRange(request.StartDate.Value, request.EndDate.Value)
            .GetInclusiveDates()
            .ToList();
        List<AccountDashboardRow> baseRows = BuildDateRows(dates, request.EndDate.Value, accountTypes);
        List<string> availableAccountNames = GetAvailableAccountNames(baseRows);
        List<AccountDashboardRow> filteredRows = ApplyFilters(
            baseRows,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        List<AccountDashboardBalanceEventRow> balanceEvents = BuildBalanceEventsForDates(
            request.StartDate.Value,
            request.EndDate.Value,
            accountTypes);
        balanceEvents = ApplyBalanceEventFilters(
            balanceEvents,
            GetApplicableAccountNames(requestedAccountNames, availableAccountNames));
        balanceEvents = SortBalanceEvents(balanceEvents, request.BalanceEventSort);
        filteredRows = SortRows(filteredRows, request.Sort);
        (decimal totalIncome, decimal totalSpending) = GetTransactionTotalsForDates(
            filteredRows.Select(row => row.Id).ToList(),
            request.StartDate.Value,
            request.EndDate.Value);
        results = new AccountDashboardModel
        {
            Mode = AccountDashboardModeModel.Date,
            Accounts = new CollectionModel<AccountDashboardAccountModel>
            {
                Items = ApplyPaging(filteredRows, request)
                    .Select(ToModel)
                    .ToList(),
                TotalCount = filteredRows.Count,
            },
            BalanceEvents = new CollectionModel<AccountDashboardBalanceEventModel>
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

    private static IEnumerable<AccountDashboardRow> ApplyPaging(
        IEnumerable<AccountDashboardRow> rows,
        AccountDashboardQueryParameterModel request) => rows
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static IEnumerable<AccountDashboardBalanceEventRow> ApplyBalanceEventPaging(
        IEnumerable<AccountDashboardBalanceEventRow> rows,
        AccountDashboardQueryParameterModel request) => rows
        .Skip(request.BalanceEventOffset ?? 0)
        .Take(request.BalanceEventLimit ?? int.MaxValue);

    private static AccountDashboardModel CreateEmptyResult(AccountDashboardModeModel mode) => new()
    {
        Mode = mode,
        Accounts = new CollectionModel<AccountDashboardAccountModel>
        {
            Items = [],
            TotalCount = 0,
        },
        BalanceEvents = new CollectionModel<AccountDashboardBalanceEventModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AvailableAccountNames = [],
        TotalIncome = 0,
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

    private List<AccountDashboardRow> BuildAccountingPeriodRows(
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

        IEnumerable<AccountDashboardRow> rows = accountsById.Values.Select(account =>
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

            return new AccountDashboardRow(
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

    private List<AccountDashboardRow> BuildDateRows(
        IReadOnlyList<DateOnly> dates,
        DateOnly endDate,
        IReadOnlySet<AccountType>? accountTypes)
    {
        IEnumerable<Account> accounts = accountRepository.GetAll()
            .Where(account => accountTypes == null || accountTypes.Contains(account.Type))
            .Where(account => account.IsOnboarded || account.DateOpened == null || account.DateOpened <= endDate);

        IEnumerable<AccountDashboardRow> rows = accounts.Select(account =>
        {
            var dateModels = dates.Select(date => new AccountDashboardDateModel
            {
                Date = date,
                Balance = GetBalanceForDate(account, date),
            }).ToList();

            return new AccountDashboardRow(
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
        IReadOnlyCollection<AccountDashboardRow> rows) => rows
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

    private static List<AccountDashboardRow> ApplyFilters(
        IReadOnlyCollection<AccountDashboardRow> rows,
        HashSet<string>? accountNames)
    {
        IEnumerable<AccountDashboardRow> filteredRows = rows;

        if (accountNames != null)
        {
            filteredRows = filteredRows.Where(row => accountNames.Contains(row.Name));
        }

        return filteredRows.ToList();
    }

    private static List<AccountDashboardBalanceEventRow> ApplyBalanceEventFilters(
        IReadOnlyCollection<AccountDashboardBalanceEventRow> rows,
        HashSet<string>? accountNames)
    {
        IEnumerable<AccountDashboardBalanceEventRow> filteredRows = rows;

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

    private static List<AccountDashboardRow> SortRows(
        List<AccountDashboardRow> rows,
        AccountDashboardSortOrderModel? sort) => sort switch
        {
            null or AccountDashboardSortOrderModel.Name => rows.OrderBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.NameDescending => rows.OrderByDescending(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.Type => rows.OrderBy(row => row.Type).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.TypeDescending => rows.OrderByDescending(row => row.Type).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.OpeningBalance => rows.OrderBy(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.OpeningBalanceDescending => rows.OrderByDescending(row => row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.ClosingBalance => rows.OrderBy(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.ClosingBalanceDescending => rows.OrderByDescending(row => row.ClosingBalance).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.NetChange => rows.OrderBy(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            AccountDashboardSortOrderModel.NetChangeDescending => rows.OrderByDescending(row => row.ClosingBalance - row.OpeningBalance).ThenBy(row => row.Name).ToList(),
            _ => rows,
        };

    private static List<AccountDashboardBalanceEventRow> SortBalanceEvents(
        List<AccountDashboardBalanceEventRow> rows,
        AccountDashboardBalanceEventSortOrderModel? sort) => sort switch
        {
            AccountDashboardBalanceEventSortOrderModel.AccountName => rows
                .OrderBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.AccountNameDescending => rows
                .OrderByDescending(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.AccountingPeriodName => rows
                .OrderBy(row => row.AccountingPeriodName)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.AccountingPeriodNameDescending => rows
                .OrderByDescending(row => row.AccountingPeriodName)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            null or AccountDashboardBalanceEventSortOrderModel.DateDescending => rows
                .OrderBy(row => row.IsPosted)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.AccountId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.Date => rows
                .OrderByDescending(row => row.IsPosted)
                .ThenBy(row => row.Date)
                .ThenBy(row => row.TransactionDate)
                .ThenBy(row => row.Sequence)
                .ThenBy(row => row.TransactionId)
                .ThenBy(row => row.AccountId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.Type => rows
                .OrderBy(row => row.Type)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.TypeDescending => rows
                .OrderByDescending(row => row.Type)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.Amount => rows
                .OrderBy(row => row.Amount)
                .ThenBy(row => row.AccountName)
                .ThenByDescending(row => row.Date)
                .ThenByDescending(row => row.TransactionDate)
                .ThenByDescending(row => row.Sequence)
                .ThenByDescending(row => row.TransactionId)
                .ThenBy(row => row.Type)
                .ToList(),
            AccountDashboardBalanceEventSortOrderModel.AmountDescending => rows
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

    private List<AccountDashboardBalanceEventRow> BuildBalanceEventsForAccountingPeriods(
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

    private List<AccountDashboardBalanceEventRow> BuildBalanceEventsForDates(
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlySet<AccountType>? accountTypes) => BuildBalanceEvents(
            transaction => true,
            accountTypes,
            effectiveDate => effectiveDate >= startDate && effectiveDate <= endDate);

    private (decimal TotalIncome, decimal TotalSpending) GetTransactionTotalsForAccountingPeriods(List<AccountId> filteredAccountIds, IReadOnlyList<AccountingPeriod> accountingPeriods)
    {
        decimal totalIncome = 0;
        decimal totalSpending = 0;

        foreach (AccountingPeriodId accountingPeriodId in accountingPeriods.Select(accountingPeriod => accountingPeriod.Id).ToHashSet())
        {
            foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(accountingPeriodId))
            {
                if (transaction is IncomeTransaction incomeTransaction && filteredAccountIds.Contains(incomeTransaction.CreditAccountId))
                {
                    totalIncome += transaction.Amount;
                }
                if (transaction is SpendingTransaction spendingTransaction && filteredAccountIds.Contains(spendingTransaction.DebitAccountId))
                {
                    totalSpending += transaction.Amount;
                }
            }
        }
        return (totalIncome, totalSpending);
    }

    private (decimal TotalIncome, decimal TotalSpending) GetTransactionTotalsForDates(List<AccountId> filteredAccountIds, DateOnly startDate, DateOnly endDate)
    {
        decimal totalIncome = 0;
        decimal totalSpending = 0;

        foreach (IncomeTransaction transaction in transactionRepository.GetAllIncomeTransactionsByDateRange(startDate, endDate).OfType<IncomeTransaction>())
        {
            if (filteredAccountIds.Contains(transaction.CreditAccountId))
            {
                totalIncome += transaction.Amount;
            }
        }
        foreach (SpendingTransaction transaction in transactionRepository.GetAllSpendingTransactionsByDateRange(startDate, endDate).OfType<SpendingTransaction>())
        {
            if (filteredAccountIds.Contains(transaction.DebitAccountId))
            {
                totalSpending += transaction.Amount;
            }
        }
        return (totalIncome, totalSpending);
    }

    private List<AccountDashboardBalanceEventRow> BuildBalanceEvents(
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

    private static IEnumerable<AccountDashboardBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Account> accountsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<AccountType>? accountTypes)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                    transaction,
                    accountsById,
                    accountingPeriodsById,
                    accountTypes,
                    spendingTransaction.DebitAccountId,
                    spendingTransaction.DebitPostedDate,
                    AccountDashboardBalanceEventTypeModel.Debit))
                {
                    yield return balanceEvent;
                }

                if (spendingTransaction.CreditAccountId != null)
                {
                    foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        spendingTransaction.CreditAccountId,
                        spendingTransaction.CreditPostedDate,
                        AccountDashboardBalanceEventTypeModel.Credit))
                    {
                        yield return balanceEvent;
                    }
                }

                break;
            case IncomeTransaction incomeTransaction:
                if (incomeTransaction.DebitAccountId != null)
                {
                    foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        incomeTransaction.DebitAccountId,
                        incomeTransaction.DebitPostedDate,
                        AccountDashboardBalanceEventTypeModel.Debit))
                    {
                        yield return balanceEvent;
                    }
                }

                foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                    transaction,
                    accountsById,
                    accountingPeriodsById,
                    accountTypes,
                    incomeTransaction.CreditAccountId,
                    incomeTransaction.CreditPostedDate,
                    AccountDashboardBalanceEventTypeModel.Credit))
                {
                    yield return balanceEvent;
                }

                break;
            case AccountTransaction accountTransaction:
                if (accountTransaction.DebitAccountId != null)
                {
                    foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        accountTransaction.DebitAccountId,
                        accountTransaction.DebitPostedDate,
                        AccountDashboardBalanceEventTypeModel.Debit))
                    {
                        yield return balanceEvent;
                    }
                }

                if (accountTransaction.CreditAccountId != null)
                {
                    foreach (AccountDashboardBalanceEventRow balanceEvent in BuildBalanceEvents(
                        transaction,
                        accountsById,
                        accountingPeriodsById,
                        accountTypes,
                        accountTransaction.CreditAccountId,
                        accountTransaction.CreditPostedDate,
                        AccountDashboardBalanceEventTypeModel.Credit))
                    {
                        yield return balanceEvent;
                    }
                }

                break;
            default:
                yield break;
        }
    }

    private static IEnumerable<AccountDashboardBalanceEventRow> BuildBalanceEvents(
        Transaction transaction,
        IReadOnlyDictionary<Guid, Account> accountsById,
        IReadOnlyDictionary<Guid, AccountingPeriod> accountingPeriodsById,
        IReadOnlySet<AccountType>? accountTypes,
        AccountId? accountId,
        DateOnly? postedDate,
        AccountDashboardBalanceEventTypeModel type)
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

        yield return new AccountDashboardBalanceEventRow(
            account.Id,
            account.Name,
            postedDate ?? transaction.Date,
            transaction.AccountingPeriodId.Value,
            accountingPeriod.Name,
            type,
            postedDate != null,
            transaction.Amount,
            transaction.Date,
            transaction.Sequence,
            transaction.Id.Value);
    }

    private static List<AccountDashboardPeriodSummaryModel> BuildPeriodSummaries(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlyCollection<AccountDashboardRow> rows) => accountingPeriods.Select(accountingPeriod =>
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

            return new AccountDashboardPeriodSummaryModel
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

    private static List<AccountDashboardDateSummaryModel> BuildDateSummaries(
        IReadOnlyList<DateOnly> dates,
        IReadOnlyCollection<AccountDashboardRow> rows) => dates.Select(date =>
        {
            List<(AccountType AccountType, decimal Balance)> balances = rows
                .Select(row =>
                {
                    AccountDashboardDateModel dateModel = row.Dates!
                        .Single(item => item.Date == date);
                    return (row.Type, dateModel.Balance);
                })
                .ToList();

            return new AccountDashboardDateSummaryModel
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

    private static AccountDashboardAccountModel ToModel(AccountDashboardRow row) => new()
    {
        Id = row.Id.Value,
        Name = row.Name,
        Type = AccountTypeConverter.ToModel(row.Type),
        StartingBalance = row.OpeningBalance,
        EndingBalance = row.ClosingBalance,
    };

    private static AccountDashboardBalanceEventModel ToModel(AccountDashboardBalanceEventRow row) => new()
    {
        AccountId = row.AccountId.Value,
        AccountName = row.AccountName,
        Date = row.Date,
        AccountingPeriodId = row.AccountingPeriodId,
        AccountingPeriodName = row.AccountingPeriodName,
        Type = row.Type,
        IsPosted = row.IsPosted,
        Amount = row.Amount,
    };

    private static decimal NormalizeBalance(AccountType accountType, decimal balance) =>
        accountType.IsDebt() ? -balance : balance;

    private sealed record AccountingPeriodBalanceValue(
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        decimal OpeningBalance,
        decimal ClosingBalance);

    private sealed record AccountDashboardRow(
        AccountId Id,
        string Name,
        AccountType Type,
        decimal OpeningBalance,
        decimal ClosingBalance,
        IReadOnlyCollection<AccountingPeriodBalanceValue>? AccountingPeriods,
        IReadOnlyCollection<AccountDashboardDateModel>? Dates);

    private sealed record AccountDashboardBalanceEventRow(
        AccountId AccountId,
        string AccountName,
        DateOnly Date,
        Guid AccountingPeriodId,
        string AccountingPeriodName,
        AccountDashboardBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}