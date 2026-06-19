using Domain;
using Domain.AccountingPeriods;
using Domain.Transactions;
using Models;
using Models.Transactions;
using Rest.AccountingPeriods;

namespace Rest.Transactions;

/// <summary>
/// Class that handles retrieving Transaction trends data for a date range or Accounting Period range.
/// </summary>
public class TransactionTrendsGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    TransactionConverter transactionConverter,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Retrieves the Transaction trends data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        TransactionTrendsQueryParameterModel request,
        out TransactionTrendsModel results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        HashSet<TransactionType>? transactionTypes = null;
        if (request.TransactionType is { Count: > 0 } requestTransactionTypes)
        {
            transactionTypes = requestTransactionTypes
                .Select(TransactionTypeConverter.ToDomain)
                .ToHashSet();
        }
        var requestedAccountNames = NormalizeNames(request.AccountName).ToHashSet();
        var requestedFundNames = NormalizeNames(request.FundName).ToHashSet();

        bool hasDateRangeParameters = request.StartDate != null || request.EndDate != null;
        bool hasAccountingPeriodRangeParameters =
            request.StartAccountingPeriodId != null || request.EndAccountingPeriodId != null;
        if (hasDateRangeParameters == hasAccountingPeriodRangeParameters)
        {
            const string message = "Provide either a date range or an Accounting Period range.";
            errors.Add(nameof(request.StartDate), [message]);
            errors.Add(nameof(request.StartAccountingPeriodId), [message]);
            results = CreateEmptyResult(TransactionTrendsModeModel.AccountingPeriod);
            return false;
        }
        if (hasDateRangeParameters)
        {
            return TryGetDateMode(request, transactionTypes, requestedAccountNames, requestedFundNames, errors, out results);
        }
        return TryGetAccountingPeriodMode(request, transactionTypes, requestedAccountNames, requestedFundNames, errors, out results);
    }

    private bool TryGetAccountingPeriodMode(
        TransactionTrendsQueryParameterModel request,
        IReadOnlySet<TransactionType>? transactionTypes,
        IReadOnlySet<string>? requestedAccountNames,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out TransactionTrendsModel results)
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
            results = CreateEmptyResult(TransactionTrendsModeModel.AccountingPeriod);
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
            results = CreateEmptyResult(TransactionTrendsModeModel.AccountingPeriod);
            return false;
        }

        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod> accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult(TransactionTrendsModeModel.AccountingPeriod);
            return false;
        }

        return BuildResult(
            request,
            transactionTypes,
            requestedAccountNames,
            requestedFundNames,
            TransactionTrendsModeModel.AccountingPeriod,
            accountingPeriods.First().GetMinimumDateInPeriod(),
            accountingPeriods.Last().GetMaximumDateInPeriod(),
            transaction => accountingPeriods.Any(accountingPeriod => accountingPeriod.Id == transaction.AccountingPeriodId),
            accountingPeriods,
            out results);
    }

    private bool TryGetDateMode(
        TransactionTrendsQueryParameterModel request,
        IReadOnlySet<TransactionType>? transactionTypes,
        IReadOnlySet<string>? requestedAccountNames,
        IReadOnlySet<string>? requestedFundNames,
        Dictionary<string, string[]> errors,
        out TransactionTrendsModel results)
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
            results = CreateEmptyResult(TransactionTrendsModeModel.Date);
            return false;
        }
        if (request.StartDate > request.EndDate)
        {
            errors.Add(nameof(request.StartDate), ["StartDate must be earlier than or equal to EndDate."]);
            results = CreateEmptyResult(TransactionTrendsModeModel.Date);
            return false;
        }

        return BuildResult(
            request,
            transactionTypes,
            requestedAccountNames,
            requestedFundNames,
            TransactionTrendsModeModel.Date,
            request.StartDate.Value,
            request.EndDate.Value,
            transaction => transaction.Date >= request.StartDate.Value && transaction.Date <= request.EndDate.Value,
            null,
            out results);
    }

    private bool BuildResult(
        TransactionTrendsQueryParameterModel request,
        IReadOnlySet<TransactionType>? transactionTypes,
        IReadOnlySet<string>? requestedAccountNames,
        IReadOnlySet<string>? requestedFundNames,
        TransactionTrendsModeModel mode,
        DateOnly startDate,
        DateOnly endDate,
        Func<Transaction, bool> transactionFilter,
        IReadOnlyList<AccountingPeriod>? accountingPeriods,
        out TransactionTrendsModel results)
    {
        var baseTransactions = transactionRepository.GetAll()
            .Where(transactionFilter)
            .Where(transaction => transactionTypes == null || transactionTypes.Contains(transaction.Type))
            .Select(transactionConverter.ToModel)
            .ToList();

        List<string> availableAccountNames = GetAvailableAccountNames(baseTransactions);
        List<string> availableFundNames = GetAvailableFundNames(baseTransactions);

        List<TransactionModel> filteredTransactions = ApplyFilters(
            baseTransactions,
            GetApplicableNames(requestedAccountNames, availableAccountNames),
            GetApplicableNames(requestedFundNames, availableFundNames));

        filteredTransactions = SortTransactions(filteredTransactions, request.Sort);

        results = new TransactionTrendsModel
        {
            Mode = mode,
            Transactions = new CollectionModel<TransactionModel>
            {
                Items = ApplyPaging(filteredTransactions, request).ToList(),
                TotalCount = filteredTransactions.Count,
            },
            AvailableAccountNames = availableAccountNames,
            AvailableFundNames = availableFundNames,
            TransactionTypes = BuildTransactionTypeSummaries(filteredTransactions),
            AccountingPeriods = mode == TransactionTrendsModeModel.AccountingPeriod && accountingPeriods != null
                ? BuildPeriodSummaries(accountingPeriods, filteredTransactions)
                : null,
            Dates = mode == TransactionTrendsModeModel.Date
                ? BuildDateSummaries(startDate, endDate, filteredTransactions)
                : null,
        };

        return true;
    }

    private static IEnumerable<TransactionModel> ApplyPaging(
        IEnumerable<TransactionModel> transactions,
        TransactionTrendsQueryParameterModel request) => transactions
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static TransactionTrendsModel CreateEmptyResult(TransactionTrendsModeModel mode) => new()
    {
        Mode = mode,
        Transactions = new CollectionModel<TransactionModel>
        {
            Items = [],
            TotalCount = 0,
        },
        AvailableAccountNames = [],
        AvailableFundNames = [],
        TransactionTypes = [],
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

    private static List<string> NormalizeNames(IReadOnlyCollection<string>? names)
    {
        if (names is not { Count: > 0 })
        {
            return [];
        }
        return names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> GetAvailableAccountNames(IReadOnlyCollection<TransactionModel> transactions) => transactions
        .SelectMany(GetAccountNamesForTransaction)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static List<string> GetAvailableFundNames(IReadOnlyCollection<TransactionModel> transactions) => transactions
        .SelectMany(GetFundNamesForTransaction)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static HashSet<string>? GetApplicableNames(IReadOnlySet<string>? requestedNames, IReadOnlyCollection<string> availableNames)
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

    private static List<TransactionModel> ApplyFilters(
        IReadOnlyCollection<TransactionModel> transactions,
        HashSet<string>? accountNames,
        HashSet<string>? fundNames)
    {
        IEnumerable<TransactionModel> filteredTransactions = transactions;

        if (accountNames != null)
        {
            filteredTransactions = filteredTransactions.Where(transaction => GetAccountNamesForTransaction(transaction).Any(accountNames.Contains));
        }

        if (fundNames != null)
        {
            filteredTransactions = filteredTransactions.Where(transaction => GetFundNamesForTransaction(transaction).Any(fundNames.Contains));
        }

        return filteredTransactions.ToList();
    }

    private static List<TransactionModel> SortTransactions(
        List<TransactionModel> transactions,
        TransactionSortOrderModel? sort) => sort switch
        {
            null or TransactionSortOrderModel.Date => transactions.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.DateDescending => transactions.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.AccountingPeriod => transactions.OrderBy(transaction => transaction.AccountingPeriodName).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.AccountingPeriodDescending => transactions.OrderByDescending(transaction => transaction.AccountingPeriodName).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.Description => transactions.OrderBy(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.DescriptionDescending => transactions.OrderByDescending(transaction => transaction.Description).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.Source => transactions.OrderBy(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.SourceDescending => transactions.OrderByDescending(GetSource).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.Destination => transactions.OrderBy(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.DestinationDescending => transactions.OrderByDescending(GetDestination).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.Amount => transactions.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList(),
            TransactionSortOrderModel.AmountDescending => transactions.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList(),
            _ => transactions,
        };

    private static List<TransactionTrendsTransactionTypeSummaryModel> BuildTransactionTypeSummaries(
        IReadOnlyCollection<TransactionModel> transactions) => transactions
        .GroupBy(transaction => transaction.TransactionType)
        .OrderBy(group => group.Key)
        .Select(group => new TransactionTrendsTransactionTypeSummaryModel
        {
            TransactionType = group.Key,
            TotalCount = group.Count(),
            TotalAmount = group.Sum(transaction => transaction.Amount),
        })
        .ToList();

    private static List<TransactionTrendsDateSummaryModel> BuildDateSummaries(
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyCollection<TransactionModel> transactions)
    {
        var transactionsByDate = transactions
            .GroupBy(transaction => transaction.Date)
            .ToDictionary(group => group.Key, group => group.ToList());

        return new DateRange(startDate, endDate)
            .GetInclusiveDates()
            .Select(date =>
            {
                _ = transactionsByDate.TryGetValue(date, out List<TransactionModel>? dateTransactions);
                dateTransactions ??= [];
                return new TransactionTrendsDateSummaryModel
                {
                    Date = date,
                    TotalCount = dateTransactions.Count,
                    TotalAmount = dateTransactions.Sum(transaction => transaction.Amount),
                };
            })
            .ToList();
    }

    private static List<TransactionTrendsPeriodSummaryModel> BuildPeriodSummaries(
        IReadOnlyList<AccountingPeriod> accountingPeriods,
        IReadOnlyCollection<TransactionModel> transactions) => accountingPeriods.Select(accountingPeriod =>
    {
        var periodTransactions = transactions
            .Where(transaction => transaction.AccountingPeriodId == accountingPeriod.Id.Value)
            .ToList();
        return new TransactionTrendsPeriodSummaryModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            TotalCount = periodTransactions.Count,
            TotalAmount = periodTransactions.Sum(transaction => transaction.Amount),
        };
    }).ToList();

    private static IEnumerable<string> GetAccountNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => new List<string?> { spendingTransaction.DebitAccount.AccountName, spendingTransaction.CreditAccount?.AccountName }.Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        IncomeTransactionModel incomeTransaction => new List<string?> { incomeTransaction.SourceAccount?.AccountName }.Concat(incomeTransaction.IncomeDestinations.Select(destination => destination.Account.AccountName)).Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        AccountTransactionModel accountTransaction => new List<string?> { accountTransaction.DebitAccount?.AccountName, accountTransaction.CreditAccount?.AccountName }.Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        _ => [],
    };

    private static IEnumerable<string> GetFundNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.FundAssignments.Select(fundAssignment => fundAssignment.FundName),
        IncomeTransactionModel incomeTransaction => incomeTransaction.IncomeDestinations.SelectMany(destination => destination.FundAssignments).Select(fundAssignment => fundAssignment.FundName),
        FundTransactionModel fundTransaction => [fundTransaction.DebitFund.FundName, fundTransaction.CreditFund.FundName],
        _ => [],
    };

    private static string? GetSource(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.SourceAccount?.AccountName ?? incomeTransaction.SourceLocation,
        AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
        _ => null,
    };

    private static string? GetDestination(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName ?? spendingTransaction.DestinationLocation,
        IncomeTransactionModel incomeTransaction => string.Join(", ", incomeTransaction.IncomeDestinations.Select(destination => destination.Account.AccountName).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
        _ => null,
    };
}