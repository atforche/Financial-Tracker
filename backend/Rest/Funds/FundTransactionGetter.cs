using System.Globalization;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Funds;
using Models;
using Models.Transactions;
using Rest.AccountingPeriods;
using Rest.Transactions;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving Transactions for a Fund based on specified criteria
/// </summary>
public class FundTransactionGetter(TransactionConverter transactionConverter, TransactionRepository transactionRepository)
{
    /// <summary>
    /// Gets the Transactions within the specified Fund that match the specified criteria
    /// </summary>
    public CollectionModel<TransactionModel> Get(FundId fundId, FundTransactionQueryParameterModel request, AccountingPeriodId? accountingPeriodId = null)
    {
        var results = transactionRepository.GetAllByFund(fundId, accountingPeriodId).Select(transactionConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(transaction =>
                transaction.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                transaction.Amount.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                AccountingPeriodTransactionGetter.GetAccountNamesForTransaction(transaction).Any(accountName => accountName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        if (request.Sort is null or FundTransactionSortOrderModel.Date)
        {
            results = results.OrderBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrderModel.DateDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrderModel.Location)
        {
            results = results.OrderBy(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrderModel.LocationDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Location).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrderModel.ChangeInBalance)
        {
            results = results.OrderBy(transaction => transaction.Amount).ThenBy(transaction => transaction.Date).ThenBy(transaction => transaction.Sequence).ToList();
        }
        else if (request.Sort == FundTransactionSortOrderModel.ChangeInBalanceDescending)
        {
            results = results.OrderByDescending(transaction => transaction.Amount).ThenByDescending(transaction => transaction.Date).ThenByDescending(transaction => transaction.Sequence).ToList();
        }
        return new CollectionModel<TransactionModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}