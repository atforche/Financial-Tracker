using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.AccountingPeriods;
using Models.Transactions.Types;

namespace Data.AccountingPeriods;

/// <summary>
/// Read-only queries for Accounting Period API models that have not yet been migrated.
/// </summary>
public sealed class AccountingPeriodQueryService(DatabaseContext databaseContext, TransactionQueryService transactionQueryService)
{
    /// <summary>
    /// Retrieves an Accounting Period and its balance by ID.
    /// </summary>
    private Task<AccountingPeriodWithBalanceModel?> GetByIdAsync(Guid accountingPeriodId, CancellationToken cancellationToken = default) =>
        (from history in databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
         where history.AccountingPeriod.Id == new AccountingPeriodId(accountingPeriodId)
         select new AccountingPeriodWithBalanceModel
         {
             Id = history.AccountingPeriod.Id.Value,
             Name = history.AccountingPeriod.Name,
             Year = history.AccountingPeriod.Year,
             Month = history.AccountingPeriod.Month,
             IsOpen = history.AccountingPeriod.IsOpen,
             OpeningBalance = history.OpeningBalance,
             ClosingBalance = history.ClosingBalance,
         }).SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Retrieves an Accounting Period with its matching Transactions and totals.
    /// </summary>
    public async Task<AccountingPeriodWithTransactionsModel?> GetWithTransactionsAsync(
        Guid accountingPeriodId,
        AccountingPeriodWithTransactionsQueryParameterModel request,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodWithBalanceModel? period = await GetByIdAsync(accountingPeriodId, cancellationToken);
        if (period == null)
        {
            return null;
        }
        var periodId = new AccountingPeriodId(accountingPeriodId);
        CollectionModel<TransactionModel> transactions = await transactionQueryService.GetForAccountingPeriodAsync(accountingPeriodId, request, cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions.AsNoTracking().OfType<IncomeTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId)
            .ToListAsync(cancellationToken);
        var incomeDestinations = incomeTransactions.SelectMany(transaction => transaction.Destinations
            .Where(destination => transaction.Source.Account == null || destination.PostedDate != null))
            .ToList();
        decimal totalIncome = incomeDestinations.Sum(destination => destination.Amount);
        decimal trackedIncome = incomeDestinations.Where(destination => destination.Account.Type.IsTracked()).Sum(destination => destination.Amount);
        decimal totalSpending = await databaseContext.Transactions.AsNoTracking().OfType<SpendingTransaction>()
            .Where(transaction => transaction.AccountingPeriodId == periodId)
            .Where(transaction => transaction.Source.PostedDate != null)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        return new AccountingPeriodWithTransactionsModel
        {
            Id = period.Id,
            Name = period.Name,
            Year = period.Year,
            Month = period.Month,
            IsOpen = period.IsOpen,
            OpeningBalance = period.OpeningBalance,
            ClosingBalance = period.ClosingBalance,
            Transactions = transactions,
            TotalIncome = new IncomeAmountModel { Total = totalIncome, Tracked = trackedIncome, Untracked = totalIncome - trackedIncome },
            TotalSpending = totalSpending,
        };
    }
}