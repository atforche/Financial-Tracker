using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;

namespace Data.Transactions;

/// <summary>
/// Entity Framework implementation of Transaction fact retrieval for balance-event queries.
/// </summary>
public sealed class TransactionBalanceEventQueryRepository(DatabaseContext databaseContext) : ITransactionBalanceEventQueryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetForAccountAsync(
        AccountId accountId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => transaction.Source.Account.Id == accountId
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Date >= startDate && transaction.Date <= endDate)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        CancellationToken cancellationToken = default) =>
        await databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetPendingForAccountsAsync(
        IReadOnlyCollection<AccountId> accountIds,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (transaction.Source.Account != null && accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (transaction.Source.Account != null && accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetPendingForFundsAsync(
        IReadOnlyCollection<FundId> fundIds,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Destinations.Any(destination => destination.PostedDate == null
                && destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => transaction.Destinations.Any(destination => destination.PostedDate == null
                && destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId))))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).ToList();
    }
}