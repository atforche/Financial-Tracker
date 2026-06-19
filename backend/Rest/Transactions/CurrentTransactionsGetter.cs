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

    private static List<string> GetAccountNamesForTransaction(TransactionModel transaction)
    {
        List<string> accountNames = [];
        switch (transaction)
        {
            case SpendingTransactionModel spendingTransaction:
                accountNames.Add(spendingTransaction.DebitAccount.AccountName);
                if (spendingTransaction.CreditAccount is not null)
                {
                    accountNames.Add(spendingTransaction.CreditAccount.AccountName);
                }
                break;
            case IncomeTransactionModel incomeTransaction:
                if (incomeTransaction.SourceAccount is not null)
                {
                    accountNames.Add(incomeTransaction.SourceAccount.AccountName);
                }
                accountNames.AddRange(incomeTransaction.IncomeDestinations.Select(destination => destination.Account.AccountName));
                break;
            case AccountTransactionModel accountTransaction:
                if (accountTransaction.DebitAccount is not null)
                {
                    accountNames.Add(accountTransaction.DebitAccount.AccountName);
                }
                if (accountTransaction.CreditAccount is not null)
                {
                    accountNames.Add(accountTransaction.CreditAccount.AccountName);
                }
                break;
            default:
                break;
        }

        return accountNames;
    }

    private static List<string> GetFundNamesForTransaction(TransactionModel transaction)
    {
        List<string> fundNames = [];
        switch (transaction)
        {
            case SpendingTransactionModel spendingTransaction:
                fundNames.AddRange(spendingTransaction.FundAssignments.Select(fundAmount => fundAmount.FundName));
                break;
            case IncomeTransactionModel incomeTransaction:
                fundNames.AddRange(incomeTransaction.IncomeDestinations.SelectMany(destination => destination.FundAssignments).Select(fundAmount => fundAmount.FundName));
                break;
            case FundTransactionModel fundTransaction:
                if (fundTransaction.DebitFund is not null)
                {
                    fundNames.Add(fundTransaction.DebitFund.FundName);
                }
                if (fundTransaction.CreditFund is not null)
                {
                    fundNames.Add(fundTransaction.CreditFund.FundName);
                }
                break;
            default:
                break;
        }

        return fundNames;
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
                accounts.Add(spendingTransaction.DebitAccount);
                if (spendingTransaction.CreditAccount is not null)
                {
                    accounts.Add(spendingTransaction.CreditAccount);
                }
                break;
            case IncomeTransactionModel incomeTransaction:
                if (incomeTransaction.SourceAccount is not null)
                {
                    accounts.Add(incomeTransaction.SourceAccount);
                }
                accounts.AddRange(incomeTransaction.IncomeDestinations.Select(destination => destination.Account));
                break;
            case AccountTransactionModel accountTransaction:
                if (accountTransaction.DebitAccount is not null)
                {
                    accounts.Add(accountTransaction.DebitAccount);
                }
                if (accountTransaction.CreditAccount is not null)
                {
                    accounts.Add(accountTransaction.CreditAccount);
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