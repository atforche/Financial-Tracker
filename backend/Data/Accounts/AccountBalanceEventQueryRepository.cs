using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Accounts;

/// <summary>
/// Entity Framework implementation of Account balance-event fact retrieval.
/// </summary>
public sealed class AccountBalanceEventQueryRepository(DatabaseContext databaseContext) : IAccountBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        AccountId accountId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => transaction.Source.Account.Id == accountId
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>()
            .AsNoTracking()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetPendingTransactionsAsync(
        AccountId accountId,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>()
            .AsNoTracking()
            .Where(transaction => (transaction.Source.Account.Id == accountId && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>()
            .AsNoTracking()
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account.Id == accountId && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>()
            .AsNoTracking()
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetTransactionsAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking()
            .Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountBalanceHistory>> GetAccountHistoriesAsync(
        IReadOnlyCollection<AccountId> ids,
        CancellationToken cancellationToken = default) =>
        await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => ids.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
}