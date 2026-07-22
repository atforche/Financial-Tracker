using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Data.Accounts;

/// <summary>
/// Entity Framework implementation of Account read operations.
/// </summary>
public sealed class AccountQueryRepository(DatabaseContext databaseContext) : IAccountQueryRepository
{
    /// <inheritdoc/>
    public async Task<QueryPage<Account>> GetAsync(AccountQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Account> accounts = ApplyFilter(databaseContext.Accounts.AsNoTracking(), query.Filter);
        accounts = query.Sort switch
        {
            AccountSort.Name => accounts.OrderBy(account => account.Name).ThenBy(account => account.Id),
            AccountSort.NameDescending => accounts.OrderByDescending(account => account.Name).ThenBy(account => account.Id),
            AccountSort.Type => accounts.OrderBy(account => account.Type).ThenBy(account => account.Name).ThenBy(account => account.Id),
            AccountSort.TypeDescending => accounts.OrderByDescending(account => account.Type).ThenBy(account => account.Name).ThenBy(account => account.Id),
            _ => accounts.OrderBy(account => account.Name).ThenBy(account => account.Id),
        };
        int totalCount = await accounts.CountAsync(cancellationToken);
        IReadOnlyCollection<Account> items = await accounts.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        return new QueryPage<Account>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<QueryPage<AccountBalance>> GetBalancesAsync(
        AccountBalanceQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Account> accounts = ApplyFilter(databaseContext.Accounts.AsNoTracking(), query.Filter);
        IQueryable<AccountBalanceRow> balances = accounts.Select(account => new AccountBalanceRow
        {
            Account = account,
            CurrentBalance = databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == account.Id)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                .Select(history => new PersistedAccountBalance
                {
                    PostedBalance = history.PostedBalance,
                    PendingDebitAmount = history.PendingDebitAmount,
                    PendingCreditAmount = history.PendingCreditAmount,
                }).FirstOrDefault() ?? new PersistedAccountBalance
                {
                    PostedBalance = account.OnboardedBalance ?? 0,
                    PendingDebitAmount = 0,
                    PendingCreditAmount = 0,
                },
        });
        balances = query.Sort switch
        {
            AccountBalanceSort.Name => balances.OrderBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
            AccountBalanceSort.NameDescending => balances.OrderByDescending(item => item.Account.Name).ThenBy(item => item.Account.Id),
            AccountBalanceSort.Type => balances.OrderBy(item => item.Account.Type).ThenBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
            AccountBalanceSort.TypeDescending => balances.OrderByDescending(item => item.Account.Type).ThenBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
            AccountBalanceSort.PostedBalance => balances.OrderBy(item => item.CurrentBalance.PostedBalance).ThenBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
            AccountBalanceSort.PostedBalanceDescending => balances.OrderByDescending(item => item.CurrentBalance.PostedBalance).ThenBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
            _ => balances.OrderBy(item => item.Account.Name).ThenBy(item => item.Account.Id),
        };
        int totalCount = await balances.CountAsync(cancellationToken);
        List<AccountBalanceRow> rows = await balances.Skip(query.Offset).Take(query.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        IReadOnlyCollection<AccountBalance> items = rows.Select(row => new AccountBalance(
            row.Account,
            row.CurrentBalance.PostedBalance,
            row.CurrentBalance.PendingDebitAmount,
            row.CurrentBalance.PendingCreditAmount)).ToList();
        return new QueryPage<AccountBalance>(items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Account>> GetRangeAccountsAsync(
        AccountFilter filter,
        CancellationToken cancellationToken = default) =>
        await ApplyFilter(databaseContext.Accounts.AsNoTracking(), filter).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<string>> GetAllNamesAsync(CancellationToken cancellationToken = default) =>
        await databaseContext.Accounts.AsNoTracking().OrderBy(account => account.Name)
            .Select(account => account.Name).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AccountPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default)
    {
        List<AccountingPeriodBalanceHistory> histories = await databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
            .Include(history => history.AccountingPeriod)
            .Include(history => history.AccountBalances).ThenInclude(balance => balance.Account)
            .Where(history => ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) >= startIndex
                && ((history.AccountingPeriod.Year * 12) + history.AccountingPeriod.Month) <= endIndex)
            .ToListAsync(cancellationToken);
        return histories.Select(history => new AccountPeriodBalanceFacts(
            history.AccountingPeriod,
            history.AccountBalances.Select(balance => new AccountPeriodBalanceFact(
                balance.Account,
                balance.OpeningBalance,
                balance.ClosingBalance)).ToList())).ToList();
    }

    /// <summary>
    /// Applies the provided filter to the queryable collection of Accounts.
    /// </summary>
    private static IQueryable<Account> ApplyFilter(IQueryable<Account> query, AccountFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.NameSearch))
        {
            query = query.Where(account => account.Name.Contains(filter.NameSearch));
        }
        if (filter.Names.Count > 0)
        {
            query = query.Where(account => filter.Names.Contains(account.Name));
        }
        if (filter.Types.Count > 0)
        {
            query = query.Where(account => filter.Types.Contains(account.Type));
        }
        return query;
    }

    /// <summary>
    /// Represents a row of account balance data, including the account and its current balance.
    /// </summary>
    private sealed class AccountBalanceRow
    {
        public required Account Account { get; init; }
        public required PersistedAccountBalance CurrentBalance { get; init; }
    }

    /// <summary>
    /// Represents the persisted balance data for an account, including posted balance and pending amounts.
    /// </summary>
    private sealed class PersistedAccountBalance
    {
        public decimal PostedBalance { get; init; }
        public decimal PendingDebitAmount { get; init; }
        public decimal PendingCreditAmount { get; init; }
    }
}