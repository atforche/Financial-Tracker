using Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Accounts;

namespace Data.Accounts;

/// <summary>
/// Read-only queries for Account API models.
/// </summary>
public sealed class AccountQueryService(DatabaseContext databaseContext)
{
    /// <summary>
    /// Retrieves Accounts matching the provided query.
    /// </summary>
    public async Task<CollectionModel<AccountModel>> GetAsync(AccountQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Account> query = ApplyFilter(databaseContext.Accounts.AsNoTracking(), request.Filter);
        query = request.Sort switch
        {
            AccountSortModel.Name => query.OrderBy(account => account.Name).ThenBy(account => account.Id),
            AccountSortModel.NameDescending => query.OrderByDescending(account => account.Name).ThenBy(account => account.Id),
            AccountSortModel.Type => query.OrderBy(account => account.Type).ThenBy(account => account.Name).ThenBy(account => account.Id),
            AccountSortModel.TypeDescending => query.OrderByDescending(account => account.Type).ThenBy(account => account.Name).ThenBy(account => account.Id),
            _ => query.OrderBy(account => account.Name).ThenBy(account => account.Id),
        };
        int totalCount = await query.CountAsync(cancellationToken);
        var accounts = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue)
            .Select(account => new { Id = account.Id.Value, account.Name, account.Type })
            .ToListAsync(cancellationToken);
        var items = accounts.Select(account => new AccountModel
        {
            Id = account.Id,
            Name = account.Name,
            Type = ToModel(account.Type),
        }).ToList();
        return new CollectionModel<AccountModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Retrieves an Account by ID.
    /// </summary>
    public async Task<AccountModel?> GetByIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await databaseContext.Accounts.AsNoTracking().Where(account => account.Id == new AccountId(accountId))
            .Select(account => new { Id = account.Id.Value, account.Name, account.Type })
            .SingleOrDefaultAsync(cancellationToken);
        return account == null
            ? null
            : new AccountModel { Id = account.Id, Name = account.Name, Type = ToModel(account.Type) };
    }

    /// <summary>
    /// Retrieves Accounts with their current balances.
    /// </summary>
    public async Task<CollectionModel<AccountWithBalanceModel>> GetWithBalancesAsync(AccountWithBalanceQueryParameterModel request, CancellationToken cancellationToken = default)
    {
        IQueryable<Account> accounts = ApplyFilter(databaseContext.Accounts.AsNoTracking(), request.Filter);
        var query = accounts.Select(account => new
        {
            Id = account.Id.Value,
            account.Name,
            account.Type,
            CurrentBalance = databaseContext.AccountBalanceHistories.Where(history => history.Account.Id == account.Id)
                .OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence)
                .Select(history => new AccountBalanceModel { PostedBalance = history.PostedBalance, PendingDebitAmount = history.PendingDebitAmount, PendingCreditAmount = history.PendingCreditAmount })
                .FirstOrDefault() ?? new AccountBalanceModel { PostedBalance = account.OnboardedBalance ?? 0, PendingDebitAmount = 0, PendingCreditAmount = 0 },
        });
        query = request.Sort switch
        {
            AccountWithBalanceSortModel.Name => query.OrderBy(account => account.Name),
            AccountWithBalanceSortModel.NameDescending => query.OrderByDescending(account => account.Name),
            AccountWithBalanceSortModel.Type => query.OrderBy(account => account.Type).ThenBy(account => account.Name),
            AccountWithBalanceSortModel.TypeDescending => query.OrderByDescending(account => account.Type).ThenBy(account => account.Name),
            AccountWithBalanceSortModel.PostedBalance => query.OrderBy(account => account.CurrentBalance.PostedBalance).ThenBy(account => account.Name),
            AccountWithBalanceSortModel.PostedBalanceDescending => query.OrderByDescending(account => account.CurrentBalance.PostedBalance).ThenBy(account => account.Name),
            _ => query.OrderBy(account => account.Name),
        };
        int totalCount = await query.CountAsync(cancellationToken);
        var accountBalances = await query.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToListAsync(cancellationToken);
        var items = accountBalances.Select(account => new AccountWithBalanceModel
        {
            Id = account.Id,
            Name = account.Name,
            Type = ToModel(account.Type),
            CurrentBalance = account.CurrentBalance,
        }).ToList();
        return new CollectionModel<AccountWithBalanceModel> { Items = items, TotalCount = totalCount };
    }

    /// <summary>
    /// Converts a domain account type after the database query has been materialized.
    /// </summary>
    private static AccountTypeModel ToModel(AccountType accountType) => accountType switch
    {
        AccountType.Standard => AccountTypeModel.Standard,
        AccountType.CreditCard => AccountTypeModel.CreditCard,
        AccountType.Investment => AccountTypeModel.Investment,
        AccountType.Debt => AccountTypeModel.Debt,
        AccountType.Retirement => AccountTypeModel.Retirement,
        AccountType.Escrow => AccountTypeModel.Escrow,
        _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType, "Unrecognized account type."),
    };

    /// <summary>
    /// Applies the filter to the provided query
    /// </summary>
    private static IQueryable<Account> ApplyFilter(IQueryable<Account> query, AccountFilterModel? filter)
    {
        if (!string.IsNullOrWhiteSpace(filter?.NameSearch))
        {
            query = query.Where(account => account.Name.Contains(filter.NameSearch));
        }
        if (filter?.Names is { Count: > 0 } names)
        {
            query = query.Where(account => names.Contains(account.Name));
        }
        if (filter?.Types is { Count: > 0 } types)
        {
            var domainTypes = types.Select(type => (AccountType)type).ToList();
            query = query.Where(account => domainTypes.Contains(account.Type));
        }
        return query;
    }
}