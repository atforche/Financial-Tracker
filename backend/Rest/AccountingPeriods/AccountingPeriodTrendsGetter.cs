using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.AccountingPeriods;
using Models.Transactions;
using Models.Transactions.Types;
using Rest.Transactions;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Accounting Period trends data.
/// </summary>
public class AccountingPeriodTrendsGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Retrieves the Accounting Period trends data that matches the specified criteria.
    /// </summary>
    public bool TryGet(
        AccountingPeriodTrendsQueryParameterModel request,
        out AccountingPeriodTrendsModel results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (!accountingPeriodConverter.TryToDomain(request.StartAccountingPeriodId, out AccountingPeriod? startAccountingPeriod))
        {
            errors.Add(
                nameof(request.StartAccountingPeriodId),
                [$"Accounting Period with ID {request.StartAccountingPeriodId} was not found."]);
        }
        if (!accountingPeriodConverter.TryToDomain(request.EndAccountingPeriodId, out AccountingPeriod? endAccountingPeriod))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                [$"Accounting Period with ID {request.EndAccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || startAccountingPeriod == null || endAccountingPeriod == null)
        {
            results = CreateEmptyResult();
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
            results = CreateEmptyResult();
            return false;
        }
        if (!TryGetAccountingPeriodsInRange(startAccountingPeriod, endAccountingPeriod, out List<AccountingPeriod>? accountingPeriods))
        {
            errors.Add(
                nameof(request.EndAccountingPeriodId),
                ["The requested Accounting Period range must be contiguous."]);
            results = CreateEmptyResult();
            return false;
        }

        var accountingPeriodModels = accountingPeriods.Select(accountingPeriodConverter.ToModel).ToList();
        accountingPeriodModels = SortAccountingPeriods(accountingPeriodModels, request.Sort);

        List<Transaction> transactions = BuildTransactionsForAccountingPeriods(accountingPeriods);
        (IncomeAmountModel totalIncome, decimal totalSpending) = GetTransactionTotals(transactions);

        var transactionModels = transactions.Select(transactionConverter.ToModel).ToList();
        transactionModels = SortTransactions(transactionModels, accountingPeriodModels.ToDictionary(ap => ap.Id, ap => ap), request.TransactionSort);

        results = new AccountingPeriodTrendsModel
        {
            AccountingPeriods = new CollectionModel<AccountingPeriodModel>
            {
                Items = ApplyPaging(accountingPeriodModels, request).ToList(),
                TotalCount = accountingPeriodModels.Count,
            },
            Transactions = new CollectionModel<TransactionModel>
            {
                Items = ApplyTransactionPaging(transactionModels, request).ToList(),
                TotalCount = transactionModels.Count,
            },
            TotalIncome = totalIncome,
            TotalSpending = totalSpending,
        };
        return true;
    }

    private static IEnumerable<AccountingPeriodModel> ApplyPaging(
        IEnumerable<AccountingPeriodModel> periods,
        AccountingPeriodTrendsQueryParameterModel request) => periods
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue);

    private static IEnumerable<TransactionModel> ApplyTransactionPaging(
        IEnumerable<TransactionModel> transactions,
        AccountingPeriodTrendsQueryParameterModel request) => transactions
        .Skip(request.TransactionOffset ?? 0)
        .Take(request.TransactionLimit ?? int.MaxValue);

    private static AccountingPeriodTrendsModel CreateEmptyResult() => new()
    {
        AccountingPeriods = new CollectionModel<AccountingPeriodModel>
        {
            Items = [],
            TotalCount = 0,
        },
        Transactions = new CollectionModel<TransactionModel>
        {
            Items = [],
            TotalCount = 0,
        },
        TotalIncome = new IncomeAmountModel
        {
            Total = 0,
            Tracked = 0,
            Untracked = 0,
        },
        TotalSpending = 0,
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

    private static List<AccountingPeriodModel> SortAccountingPeriods(
        List<AccountingPeriodModel> accountingPeriods,
        AccountingPeriodSortOrderModel? sort) => sort switch
        {
            null or AccountingPeriodSortOrderModel.Date => accountingPeriods
                .OrderBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.DateDescending => accountingPeriods
                .OrderByDescending(period => period.Year)
                .ThenByDescending(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.IsOpen => accountingPeriods
                .OrderBy(period => period.IsOpen)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.IsOpenDescending => accountingPeriods
                .OrderByDescending(period => period.IsOpen)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.OpeningBalance => accountingPeriods
                .OrderBy(period => period.OpeningBalance)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.OpeningBalanceDescending => accountingPeriods
                .OrderByDescending(period => period.OpeningBalance)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.ClosingBalance => accountingPeriods
                .OrderBy(period => period.ClosingBalance)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            AccountingPeriodSortOrderModel.ClosingBalanceDescending => accountingPeriods
                .OrderByDescending(period => period.ClosingBalance)
                .ThenBy(period => period.Year)
                .ThenBy(period => period.Month)
                .ToList(),
            _ => accountingPeriods,
        };

    private List<Transaction> BuildTransactionsForAccountingPeriods(
        IReadOnlyCollection<AccountingPeriod> accountingPeriods) => accountingPeriods
        .SelectMany(accountingPeriod => transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id))
        .ToList();

    private static (IncomeAmountModel TotalIncome, decimal TotalSpending) GetTransactionTotals(
        IReadOnlyCollection<Transaction> transactions)
    {
        decimal trackedIncome = 0;
        decimal untrackedIncome = 0;
        decimal totalSpending = 0;

        foreach (Transaction transaction in transactions)
        {
            if (transaction is IncomeTransaction incomeTransaction)
            {
                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    if (destination.Account != null && destination.Account.Type.IsTracked())
                    {
                        trackedIncome += destination.Amount;
                    }
                    else
                    {
                        untrackedIncome += destination.Amount;
                    }
                }
            }
            else if (transaction is SpendingTransaction)
            {
                totalSpending += transaction.Amount;
            }
        }

        return (
            new IncomeAmountModel
            {
                Total = trackedIncome + untrackedIncome,
                Tracked = trackedIncome,
                Untracked = untrackedIncome,
            },
            totalSpending);
    }

    private static List<TransactionModel> SortTransactions(
        List<TransactionModel> transactions,
        Dictionary<Guid, AccountingPeriodModel> accountingPeriodsById,
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
            TransactionSortOrderModel.AccountingPeriod => transactions
                .OrderBy(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Year)
                .ThenBy(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Month)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.AccountingPeriodDescending => transactions
                .OrderByDescending(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Year)
                .ThenByDescending(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Month)
                .ThenByDescending(transaction => transaction.Date)
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