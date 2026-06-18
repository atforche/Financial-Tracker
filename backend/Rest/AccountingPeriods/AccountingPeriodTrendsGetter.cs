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
        (decimal totalIncome, decimal totalSpending) = GetTransactionTotals(transactions);

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
        TotalIncome = 0,
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
        Dictionary<Guid, AccountingPeriodModel> accountingPeriodsById,
        AccountingPeriodTrendsTransactionSortOrderModel? sort) => sort switch
        {
            null or AccountingPeriodTrendsTransactionSortOrderModel.Date => transactions
                .OrderBy(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.DateDescending => transactions
                .OrderByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.AccountingPeriod => transactions
                .OrderBy(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Year)
                .ThenBy(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Month)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.AccountingPeriodDescending => transactions
                .OrderByDescending(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Year)
                .ThenByDescending(transaction => accountingPeriodsById[transaction.AccountingPeriodId].Month)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.Description => transactions
                .OrderBy(transaction => transaction.Description)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.DescriptionDescending => transactions
                .OrderByDescending(transaction => transaction.Description)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.Source => transactions
                .OrderBy(GetSource)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.SourceDescending => transactions
                .OrderByDescending(GetSource)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.Destination => transactions
                .OrderBy(GetDestination)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.DestinationDescending => transactions
                .OrderByDescending(GetDestination)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.Amount => transactions
                .OrderBy(transaction => transaction.Amount)
                .ThenBy(transaction => transaction.Date)
                .ThenBy(transaction => transaction.Sequence)
                .ToList(),
            AccountingPeriodTrendsTransactionSortOrderModel.AmountDescending => transactions
                .OrderByDescending(transaction => transaction.Amount)
                .ThenByDescending(transaction => transaction.Date)
                .ThenByDescending(transaction => transaction.Sequence)
                .ToList(),
            _ => transactions,
        };

    private static string? GetSource(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
        IncomeTransactionModel incomeTransaction => incomeTransaction.DebitAccount?.AccountName ?? incomeTransaction.SourceLocation,
        AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
        _ => null,
    };

    private static string? GetDestination(TransactionModel transaction) => transaction switch
    {
        SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName ?? spendingTransaction.DestinationLocation,
        IncomeTransactionModel incomeTransaction => incomeTransaction.CreditAccount.AccountName,
        AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
        FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
        _ => null,
    };
}