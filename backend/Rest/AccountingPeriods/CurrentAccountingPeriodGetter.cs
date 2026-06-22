using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models;
using Models.AccountingPeriods;
using Models.Transactions;
using Rest.Transactions;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving current Accounting Period data.
/// </summary>
public class CurrentAccountingPeriodGetter(
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Retrieves the current Accounting Period data that matches the specified criteria.
    /// </summary>
    public CurrentAccountingPeriodModel Get(CurrentAccountingPeriodQueryParameterModel request)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetLatestAccountingPeriod();
        if (accountingPeriod == null)
        {
            return CreateEmptyResult();
        }

        var transactions = transactionRepository.GetAllByAccountingPeriod(accountingPeriod.Id).ToList();
        (IncomeAmountModel totalIncome, decimal totalSpending) = GetTransactionTotals(transactions);
        List<TransactionModel> transactionModels = SortTransactions(
            transactions.Select(transactionConverter.ToModel).ToList(),
            request.TransactionSort);

        return new CurrentAccountingPeriodModel
        {
            AccountingPeriod = accountingPeriodConverter.ToModel(accountingPeriod),
            Transactions = new CollectionModel<TransactionModel>
            {
                Items = ApplyTransactionPaging(transactionModels, request).ToList(),
                TotalCount = transactionModels.Count,
            },
            TotalIncome = totalIncome,
            TotalSpending = totalSpending,
        };
    }

    private static CurrentAccountingPeriodModel CreateEmptyResult() => new()
    {
        AccountingPeriod = null,
        Transactions = new CollectionModel<TransactionModel>
        {
            Items = [],
            TotalCount = 0,
        },
        TotalIncome = new IncomeAmountModel
        {
            Total = 0,
            Tracked = 0,
            Untracked = 0
        },
        TotalSpending = 0,
    };

    private static IEnumerable<TransactionModel> ApplyTransactionPaging(
        IEnumerable<TransactionModel> transactions,
        CurrentAccountingPeriodQueryParameterModel request) => transactions
        .Skip(request.TransactionOffset ?? 0)
        .Take(request.TransactionLimit ?? int.MaxValue);

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
                if (incomeTransaction.Source.Account != null && incomeTransaction.Source.Account.Type.IsTracked())
                {
                    trackedIncome += transaction.Amount;
                }
                else
                {
                    untrackedIncome += transaction.Amount;
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
                Untracked = untrackedIncome
            },
            totalSpending);
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