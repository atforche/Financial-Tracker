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

        var transactions = transactionRepository
            .GetAllByAccountingPeriod(accountingPeriod.Id)
            .Select(transactionConverter.ToModel)
            .ToList();

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
                if (incomeTransaction.DebitAccount is not null)
                {
                    accounts.Add(incomeTransaction.DebitAccount);
                }
                accounts.Add(incomeTransaction.CreditAccount);
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
            TransactionSortOrderModel.Location => transactions
                .OrderBy(transaction => transaction.Location)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.LocationDescending => transactions
                .OrderByDescending(transaction => transaction.Location)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.DebitFrom => transactions
                .OrderBy(GetDebitFrom)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.DebitFromDescending => transactions
                .OrderByDescending(GetDebitFrom)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.CreditTo => transactions
                .OrderBy(GetCreditTo)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            TransactionSortOrderModel.CreditToDescending => transactions
                .OrderByDescending(GetCreditTo)
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

    private static string? GetDebitFrom(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.DebitAccount?.AccountName,
        AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
        _ => null,
    };

    private static string? GetCreditTo(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.CreditAccount.AccountName,
        AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
        _ => null,
    };
}