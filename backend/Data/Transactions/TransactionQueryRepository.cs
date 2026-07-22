using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Queries;
using Microsoft.EntityFrameworkCore;

namespace Data.Transactions;

/// <summary>
/// Entity Framework implementation of Transaction detail fact retrieval.
/// </summary>
public sealed class TransactionQueryRepository(DatabaseContext databaseContext) : ITransactionQueryRepository
{
    /// <inheritdoc/>
    public async Task<TransactionDetailsFacts?> GetDetailsByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await databaseContext.Transactions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == transactionId, cancellationToken);
        if (transaction == null)
        {
            return null;
        }

        AccountingPeriod period = await databaseContext.AccountingPeriods.AsNoTracking()
            .SingleAsync(item => item.Id == transaction.AccountingPeriodId, cancellationToken);
        IReadOnlyCollection<FundId> fundIds = transaction.GetAllAffectedFundIds(null).Distinct().ToList();
        IReadOnlyCollection<Fund> funds = await databaseContext.Funds.AsNoTracking()
            .Where(fund => fundIds.Contains(fund.Id))
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountId> accountIds = transaction.GetAllAffectedAccountIds().Distinct().ToList();
        IReadOnlyCollection<AccountBalanceHistory> accountHistories = await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => accountIds.Contains(history.Account.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundBalanceHistory> fundHistories = await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.Fund.Id))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        IReadOnlyCollection<FundPlanTotalsHistory> fundPlanHistories = await databaseContext.FundPlanTotalsHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence)
            .ToListAsync(cancellationToken);
        return new TransactionDetailsFacts(
            transaction,
            period,
            funds,
            accountHistories,
            fundHistories,
            fundPlanHistories);
    }
}