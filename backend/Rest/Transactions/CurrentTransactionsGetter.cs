using Domain.AccountingPeriods;
using Domain.Transactions;
using Models;
using Models.Transactions;

namespace Rest.Transactions;

/// <summary>
/// Class that handles retrieving current Transactions data.
/// </summary>
public class CurrentTransactionsGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Retrieves the current Transactions page data.
    /// </summary>
    public CurrentTransactionsModel Get(CurrentTransactionsQueryParameterModel request)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (accountingPeriod is null)
        {
            return CreateEmptyResult();
        }

        HashSet<TransactionType>? transactionTypes = null;
        if (request.TransactionType is { Count: > 0 } requestTransactionTypes)
        {
            transactionTypes = requestTransactionTypes
                .Select(TransactionTypeConverter.ToDomain)
                .ToHashSet();
        }

        HashSet<string>? requestedAccountNames = NormalizeNames(request.AccountName);
        HashSet<string>? requestedFundNames = NormalizeNames(request.FundName);

        var baseTransactions = transactionRepository
            .GetAllByAccountingPeriod(accountingPeriod.Id)
            .Where(transaction => transactionTypes == null || transactionTypes.Contains(transaction.Type))
            .Select(transactionConverter.ToModel)
            .ToList();

        List<string> availableAccountNames = GetAvailableAccountNames(baseTransactions);
        List<string> availableFundNames = GetAvailableFundNames(baseTransactions);

        List<TransactionModel> transactions = ApplyFilters(
            baseTransactions,
            GetApplicableNames(requestedAccountNames, availableAccountNames),
            GetApplicableNames(requestedFundNames, availableFundNames));

        List<TransactionModel> unpostedTransactions = SortTransactions(
            transactions.Where(transaction => !IsFullyPosted(transaction)).ToList(),
            request.UnpostedTransactionSort);
        List<TransactionModel> postedTransactions = SortTransactions(
            transactions.Where(IsFullyPosted).ToList(),
            request.PostedTransactionSort);

        return new CurrentTransactionsModel
        {
            AccountingPeriodId = accountingPeriod.Id.Value,
            AccountingPeriodName = accountingPeriod.Name,
            AvailableAccountNames = availableAccountNames,
            AvailableFundNames = availableFundNames,
            TransactionTypes = BuildTransactionTypeSummaries(transactions),
            UnpostedTransactions = new CollectionModel<TransactionModel>
            {
                Items = unpostedTransactions
                    .Skip(request.UnpostedTransactionOffset ?? 0)
                    .Take(request.UnpostedTransactionLimit ?? int.MaxValue)
                    .ToList(),
                TotalCount = unpostedTransactions.Count,
            },
            PostedTransactions = new CollectionModel<TransactionModel>
            {
                Items = postedTransactions
                    .Skip(request.PostedTransactionOffset ?? 0)
                    .Take(request.PostedTransactionLimit ?? int.MaxValue)
                    .ToList(),
                TotalCount = postedTransactions.Count,
            },
        };
    }

    private static CurrentTransactionsModel CreateEmptyResult() => new()
    {
        AccountingPeriodId = null,
        AccountingPeriodName = null,
        AvailableAccountNames = [],
        AvailableFundNames = [],
        TransactionTypes = [],
        UnpostedTransactions = new CollectionModel<TransactionModel>
        {
            Items = [],
            TotalCount = 0,
        },
        PostedTransactions = new CollectionModel<TransactionModel>
        {
            Items = [],
            TotalCount = 0,
        },
    };

    private static HashSet<string>? NormalizeNames(IReadOnlyCollection<string>? names)
    {
        if (names is not { Count: > 0 })
        {
            return null;
        }

        var normalizedNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return normalizedNames.Count == 0 ? null : normalizedNames;
    }

    private static HashSet<string>? GetApplicableNames(
        IReadOnlySet<string>? requestedNames,
        IReadOnlyCollection<string> availableNames)
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

    private static List<TransactionTrendsTransactionTypeSummaryModel> BuildTransactionTypeSummaries(
        IReadOnlyCollection<TransactionModel> transactions) => transactions
        .GroupBy(transaction => transaction.TransactionType)
        .Select(grouping => new TransactionTrendsTransactionTypeSummaryModel
        {
            TransactionType = grouping.Key,
            TotalCount = grouping.Count(),
            TotalAmount = grouping.Sum(transaction => transaction.Amount),
        })
        .OrderBy(summary => summary.TransactionType.ToString(), StringComparer.OrdinalIgnoreCase)
        .ToList();

    private static bool IsFullyPosted(TransactionModel transaction)
    {
        List<TransactionAccountModel> accounts = GetAccounts(transaction);
        return accounts.Count == 0 || accounts.All(account => account.PostedDate != null);
    }

    private static List<TransactionAccountModel> GetAccounts(TransactionModel transaction)
    {
        List<TransactionAccountModel> accounts = [];

        switch (transaction)
        {
            case SpendingTransactionModel spendingTransaction:
                accounts.Add(spendingTransaction.Source.Account);
                foreach (SpendingTransactionDestinationModel destination in spendingTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        accounts.Add(destination.Account);
                    }
                }
                break;
            case IncomeTransactionModel incomeTransaction:
                if (incomeTransaction.Source.Account is not null)
                {
                    accounts.Add(incomeTransaction.Source.Account);
                }
                foreach (IncomeTransactionDestinationModel destination in incomeTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        accounts.Add(destination.Account);
                    }
                }
                break;
            case AccountTransactionModel accountTransaction:
                if (accountTransaction.Source.Account is not null)
                {
                    accounts.Add(accountTransaction.Source.Account);
                }
                foreach (AccountTransactionDestinationModel destination in accountTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        accounts.Add(destination.Account);
                    }
                }
                break;
            default:
                break;
        }

        return accounts;
    }

    private static List<TransactionModel> SortTransactions(
        List<TransactionModel> transactions,
        TransactionSortOrderModel? sort) => sort switch
        {
            null or TransactionSortOrderModel.Date => transactions
                .OrderBy(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.DateDescending => transactions
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.AccountingPeriod or
            TransactionSortOrderModel.AccountingPeriodDescending => transactions
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.Description => transactions
                .OrderBy(transaction => transaction.Description)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.DescriptionDescending => transactions
                .OrderByDescending(transaction => transaction.Description)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.Source => transactions
                .OrderBy(GetSource)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.SourceDescending => transactions
                .OrderByDescending(GetSource)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.Destination => transactions
                .OrderBy(GetDestination)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.DestinationDescending => transactions
                .OrderByDescending(GetDestination)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.Amount => transactions
                .OrderBy(transaction => transaction.Amount)
                .ThenBy(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.AmountDescending => transactions
                .OrderByDescending(transaction => transaction.Amount)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            _ => transactions,
        };

    private static IEnumerable<string> GetAccountNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(spendingTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        IncomeTransactionModel incomeTransaction => incomeTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(incomeTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        AccountTransactionModel accountTransaction => accountTransaction.Destinations.Select(destination => destination.Account?.AccountName)
            .Append(accountTransaction.Source.Account?.AccountName)
            .Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name!),
        _ => [],
    };

    private static IEnumerable<string> GetFundNamesForTransaction(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Destinations.SelectMany(destination => destination.FundAssignments).Select(fundAssignment => fundAssignment.FundName),
        IncomeTransactionModel incomeTransaction => incomeTransaction.Destinations.SelectMany(destination => destination.FundAssignments).Select(fundAssignment => fundAssignment.FundName),
        FundTransactionModel fundTransaction => fundTransaction.Destinations.Select(destination => destination.Fund.FundName).Append(fundTransaction.Source.Fund.FundName),
        _ => [],
    };

    private static string? GetSource(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.Source.Account.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.Source.Account?.AccountName ?? incomeTransaction.Source.Location,
        AccountTransactionModel accountTransaction => accountTransaction.Source.Account?.AccountName ?? accountTransaction.Source.Location,
        FundTransactionModel fundTransaction => fundTransaction.Source.Fund?.FundName,
        _ => null,
    };

    private static string? GetDestination(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => string.Join(", ", spendingTransaction.Destinations.Select(destination => destination.Account?.AccountName ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        IncomeTransactionModel incomeTransaction => string.Join(", ", incomeTransaction.Destinations.Select(destination => destination.Account?.AccountName).Distinct(StringComparer.OrdinalIgnoreCase)),
        AccountTransactionModel accountTransaction => string.Join(", ", accountTransaction.Destinations.Select(destination => destination.Account?.AccountName ?? destination.Location).Distinct(StringComparer.OrdinalIgnoreCase)),
        FundTransactionModel fundTransaction => string.Join(", ", fundTransaction.Destinations.Select(destination => destination.Fund?.FundName).Distinct(StringComparer.OrdinalIgnoreCase)),
        _ => null,
    };
}