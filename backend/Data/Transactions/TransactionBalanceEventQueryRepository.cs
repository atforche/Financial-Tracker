using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Refunds;
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
        DateOnly? startDate,
        DateOnly? endDate,
        IReadOnlyCollection<AccountingPeriodId>? accountingPeriodIds,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (startDate == null || transaction.Date >= startDate)
                && (endDate == null || transaction.Date <= endDate)
                && (accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId)))
            .Where(transaction => transaction.Source.Account.Id == accountId
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (startDate == null || transaction.Date >= startDate)
                && (endDate == null || transaction.Date <= endDate)
                && (accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId)))
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (startDate == null || transaction.Date >= startDate)
                && (endDate == null || transaction.Date <= endDate)
                && (accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId)))
            .Where(transaction => (transaction.Source.Account != null && transaction.Source.Account.Id == accountId)
                || transaction.Destinations.Any(destination => destination.Account != null && destination.Account.Id == accountId))
            .ToListAsync(cancellationToken);
        List<RefundTransaction> refundTransactions = await databaseContext.Transactions.OfType<RefundTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => (startDate == null || transaction.Date >= startDate)
                && (endDate == null || transaction.Date <= endDate)
                && (accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId)))
            .Where(transaction => transaction.Sources.Any(source => source.Account != null && source.Account.Id == accountId) || transaction.Destination.Account.Id == accountId)
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).Concat(refundTransactions).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetForAccountPostingsAsync(
        AccountId accountId, DateOnly? startDate, DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TransactionId> ids = databaseContext.AccountBalanceHistories
            .Where(history => history.Account.Id == accountId
                && (startDate == null || history.Date >= startDate)
                && (endDate == null || history.Date <= endDate))
            .Select(history => history.TransactionId).Distinct();
        return await databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .Where(transaction => ids.Contains(transaction.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetForFundPostingsAsync(
        DateOnly? startDate, DateOnly? endDate,
        IReadOnlyCollection<FundId>? fundIds,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TransactionId> ids = databaseContext.FundBalanceHistories
            .Where(history => (startDate == null || history.Date >= startDate)
                && (endDate == null || history.Date <= endDate)
                && (fundIds == null || fundIds.Contains(history.Fund.Id)))
            .Select(history => history.TransactionId).Distinct();
        return await databaseContext.Transactions.AsNoTracking().AsSplitQuery()
            .Where(transaction => ids.Contains(transaction.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Transaction>> GetForFundsAsync(
        IReadOnlyCollection<AccountingPeriodId> accountingPeriodIds,
        IReadOnlyCollection<FundId> fundIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Transaction> assignedTransactions = await GetWithFundAssignmentsAsync(accountingPeriodIds, fundIds, false, cancellationToken);
        List<FundTransaction> fundTransactions = await databaseContext.Transactions
            .OfType<FundTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => fundIds.Contains(transaction.Source.Fund.Id)
                || transaction.Destinations.Any(destination => fundIds.Contains(destination.Fund.Id)))
            .ToListAsync(cancellationToken);
        return assignedTransactions.Concat(fundTransactions).ToList();
    }

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
        IReadOnlyCollection<AccountingPeriodId>? accountingPeriodIds = null,
        CancellationToken cancellationToken = default)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => (accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => (transaction.Source.Account != null && accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<AccountTransaction> accountTransactions = await databaseContext.Transactions
            .OfType<AccountTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => (transaction.Source.Account != null && accountIds.Contains(transaction.Source.Account.Id) && transaction.Source.PostedDate == null)
                || transaction.Destinations.Any(destination => destination.Account != null && accountIds.Contains(destination.Account.Id) && destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        List<RefundTransaction> refundTransactions = await databaseContext.Transactions.OfType<RefundTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => transaction.Sources.Any(source => source.Account != null && accountIds.Contains(source.Account.Id) && source.PostedDate == null) || (accountIds.Contains(transaction.Destination.Account.Id) && transaction.Destination.PostedDate == null))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(accountTransactions).Concat(refundTransactions).ToList();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyCollection<Transaction>> GetPendingForFundsAsync(
        IReadOnlyCollection<FundId>? fundIds,
        CancellationToken cancellationToken = default) =>
        GetWithFundAssignmentsAsync(null, fundIds, true, cancellationToken);

    /// <summary>
    /// Retrieves the Transactions that assign amounts to Funds, optionally narrowed to Accounting Periods, to Funds, and to
    /// the assignments that haven't posted.
    /// </summary>
    private async Task<IReadOnlyCollection<Transaction>> GetWithFundAssignmentsAsync(
        IReadOnlyCollection<AccountingPeriodId>? accountingPeriodIds,
        IReadOnlyCollection<FundId>? fundIds,
        bool pendingOnly,
        CancellationToken cancellationToken)
    {
        List<SpendingTransaction> spendingTransactions = await databaseContext.Transactions
            .OfType<SpendingTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => transaction.Destinations.Any(destination => (!pendingOnly || destination.PostedDate == null)
                && (fundIds == null || destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId)))))
            .ToListAsync(cancellationToken);
        List<IncomeTransaction> incomeTransactions = await databaseContext.Transactions
            .OfType<IncomeTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => transaction.Destinations.Any(destination => (!pendingOnly || destination.PostedDate == null)
                && (fundIds == null || destination.FundAssignments.Any(amount => fundIds.Contains(amount.FundId)))))
            .ToListAsync(cancellationToken);
        List<RefundTransaction> refundTransactions = await databaseContext.Transactions
            .OfType<RefundTransaction>().AsNoTracking().AsSplitQuery()
            .Where(transaction => accountingPeriodIds == null || accountingPeriodIds.Contains(transaction.AccountingPeriodId))
            .Where(transaction => transaction.Sources.Any(source =>
                (!pendingOnly || (source.Account == null ? transaction.Destination.PostedDate : source.PostedDate) == null)
                && (fundIds == null || source.FundAssignments.Any(amount => fundIds.Contains(amount.FundId)))))
            .ToListAsync(cancellationToken);
        return spendingTransactions.Concat<Transaction>(incomeTransactions).Concat(refundTransactions).ToList();
    }
}
