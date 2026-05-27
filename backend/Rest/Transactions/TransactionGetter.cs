using System.Globalization;
using Domain.AccountingPeriods;
using Domain.Transactions;
using Models;
using Models.Transactions;
using Rest.AccountingPeriods;

namespace Rest.Transactions;

/// <summary>
/// Class that handles retrieving top-level Transactions based on specified criteria
/// </summary>
public class TransactionGetter(
    ITransactionRepository transactionRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    TransactionConverter transactionConverter)
{
    /// <summary>
    /// Gets the Transactions that match the specified criteria
    /// </summary>
    public bool TryGet(
        TransactionQueryParameterModel request,
        out CollectionModel<TransactionModel> results,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        AccountingPeriodId? accountingPeriodId = null;
        if (request.AccountingPeriodId is Guid requestAccountingPeriodId)
        {
            if (!accountingPeriodConverter.TryToDomain(requestAccountingPeriodId, out AccountingPeriod? accountingPeriod))
            {
                errors.Add(
                    nameof(request.AccountingPeriodId),
                    [$"Accounting Period with ID {requestAccountingPeriodId} was not found."]);
                results = new CollectionModel<TransactionModel>
                {
                    Items = [],
                    TotalCount = 0,
                };
                return false;
            }

            accountingPeriodId = accountingPeriod.Id;
        }

        var filteredResults = (accountingPeriodId is null
                ? transactionRepository.GetAll()
                : transactionRepository.GetAllByAccountingPeriod(accountingPeriodId))
            .Select(transactionConverter.ToModel)
            .ToList();

        if (request.Search != null)
        {
            filteredResults = filteredResults.Where(transaction =>
                    transaction.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    transaction.Amount.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                    GetAccountNamesForTransaction(transaction).Any(accountName => accountName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (request.Sort is null or TransactionSortOrderModel.Date)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DateDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Location)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.LocationDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DebitFrom)
        {
            filteredResults = filteredResults.OrderBy(GetDebitFrom).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.DebitFromDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetDebitFrom).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.CreditTo)
        {
            filteredResults = filteredResults.OrderBy(GetCreditTo).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.CreditToDescending)
        {
            filteredResults = filteredResults.OrderByDescending(GetCreditTo).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.Amount)
        {
            filteredResults = filteredResults.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == TransactionSortOrderModel.AmountDescending)
        {
            filteredResults = filteredResults.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }

        results = new CollectionModel<TransactionModel>
        {
            Items = filteredResults.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = filteredResults.Count,
        };
        return true;
    }

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

    private static IEnumerable<string> GetAccountNamesForTransaction(TransactionModel transaction)
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