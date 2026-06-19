using Domain.AccountingPeriods;
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
        (decimal totalIncome, decimal totalSpending) = GetTransactionTotals(transactions);
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
        TotalIncome = 0,
        TotalSpending = 0,
    };

    private static IEnumerable<TransactionModel> ApplyTransactionPaging(
        IEnumerable<TransactionModel> transactions,
        CurrentAccountingPeriodQueryParameterModel request) => transactions
        .Skip(request.TransactionOffset ?? 0)
        .Take(request.TransactionLimit ?? int.MaxValue);

    private static (decimal TotalIncome, decimal TotalSpending) GetTransactionTotals(
        IReadOnlyCollection<Transaction> transactions)
    {
        decimal totalIncome = 0;
        decimal totalSpending = 0;

        foreach (Transaction transaction in transactions)
        {
            if (transaction is IncomeTransaction)
            {
                totalIncome += transaction.Amount;
            }
            else if (transaction is SpendingTransaction)
            {
                totalSpending += transaction.Amount;
            }
        }

        return (totalIncome, totalSpending);
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