using System.Globalization;
using Domain.AccountingPeriods;
using Domain.Transactions;
using Models;
using Models.AccountingPeriods;
using Models.Transactions;
using Rest.Transactions;

namespace Rest.AccountingPeriods;

/// <summary>
/// Class that handles retrieving Transactions for an Accounting Period based on specified criteria
/// </summary>
public class AccountingPeriodTransactionGetter(
    ITransactionRepository transactionRepository,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Gets the Transactions within the specified Accounting Period that match the specified criteria
    /// </summary>
    public CollectionModel<TransactionModel> Get(AccountingPeriodId accountingPeriodId, AccountingPeriodTransactionQueryParameterModel request)
    {
        var results = transactionRepository.GetAllByAccountingPeriod(accountingPeriodId).Select(transactionConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(transaction =>
                transaction.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                transaction.Amount.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                GetAccountNamesForTransaction(transaction).Any(accountName => accountName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        if (request.Sort is null or AccountingPeriodTransactionSortOrderModel.Date)
        {
            results = results.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.DateDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.Location)
        {
            results = results.OrderBy(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.LocationDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.DebitFrom)
        {
            results = results.OrderByDescending(transaction => transaction switch
            {
                SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
                IncomeTransactionModel incomeTransaction => incomeTransaction.DebitAccount?.AccountName,
                AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
                FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
                _ => null
            })
            .ThenByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Sequence)
            .ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.DebitFromDescending)
        {
            results = results.OrderByDescending(transaction => transaction switch
            {
                SpendingTransactionModel spendingTransaction => spendingTransaction.DebitAccount.AccountName,
                IncomeTransactionModel incomeTransaction => incomeTransaction.DebitAccount?.AccountName,
                AccountTransactionModel accountTransaction => accountTransaction.DebitAccount?.AccountName,
                FundTransactionModel fundTransaction => fundTransaction.DebitFund?.FundName,
                _ => null
            })
            .ThenByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Sequence)
            .ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.CreditTo)
        {
            results = results.OrderByDescending(transaction => transaction switch
            {
                SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName,
                IncomeTransactionModel incomeTransaction => incomeTransaction.CreditAccount.AccountName,
                AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
                FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
                _ => null
            })
            .ThenByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Sequence)
            .ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.CreditToDescending)
        {
            results = results.OrderByDescending(transaction => transaction switch
            {
                SpendingTransactionModel spendingTransaction => spendingTransaction.CreditAccount?.AccountName,
                IncomeTransactionModel incomeTransaction => incomeTransaction.CreditAccount.AccountName,
                AccountTransactionModel accountTransaction => accountTransaction.CreditAccount?.AccountName,
                FundTransactionModel fundTransaction => fundTransaction.CreditFund?.FundName,
                _ => null
            })
            .ThenByDescending(transaction => transaction.Date)
            .ThenByDescending(transaction => transaction.Sequence)
            .ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.Amount)
        {
            results = results.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == AccountingPeriodTransactionSortOrderModel.AmountDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        return new CollectionModel<TransactionModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }

    /// <summary>
    /// Gets the account names for the accounts affected by the provided TransactionModel
    /// </summary>
    public static IEnumerable<string> GetAccountNamesForTransaction(TransactionModel transaction)
    {
        if (transaction is SpendingTransactionModel spendingTransaction)
        {
            yield return spendingTransaction.DebitAccount.AccountName;
            if (spendingTransaction.CreditAccount != null)
            {
                yield return spendingTransaction.CreditAccount.AccountName;
            }
        }
        else if (transaction is IncomeTransactionModel incomeTransaction)
        {
            if (incomeTransaction.DebitAccount != null)
            {
                yield return incomeTransaction.DebitAccount.AccountName;
            }
            yield return incomeTransaction.CreditAccount.AccountName;
        }
        else if (transaction is AccountTransactionModel accountTransaction)
        {
            if (accountTransaction.DebitAccount != null)
            {
                yield return accountTransaction.DebitAccount.AccountName;
            }
            if (accountTransaction.CreditAccount != null)
            {
                yield return accountTransaction.CreditAccount.AccountName;
            }
        }
    }
}