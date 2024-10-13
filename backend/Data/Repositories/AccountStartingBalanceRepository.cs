using Data.EntityModels;
using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Account Starting Balance repository that allows Account Starting Balances to be persisted to the database
/// </summary>
public class AccountStartingBalanceRepository : IAccountStartingBalanceRepository
{
    private readonly DatabaseContext _context;
    private readonly IAccountStartingBalanceFactory _accountStartingBalanceFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    /// <param name="accountStartingBalanceFactory">Factory used to construct Account Starting Balance instances</param>
    public AccountStartingBalanceRepository(
        DatabaseContext context,
        IAccountStartingBalanceFactory accountStartingBalanceFactory)
    {
        _context = context;
        _accountStartingBalanceFactory = accountStartingBalanceFactory;
    }

    /// <inheritdoc/>
    public AccountStartingBalance? FindOrNull(Guid accountId, Guid accountingPeriodId)
    {
        AccountStartingBalanceData? accountStartingBalanceData = _context.AccountStartingBalances
            .SingleOrDefault(accountStartingBalance => accountStartingBalance.AccountId == accountId &&
                accountStartingBalance.AccountingPeriodId == accountingPeriodId);
        return accountStartingBalanceData != null ? ConvertToEntity(accountStartingBalanceData) : null;
    }

    /// <inheritdoc/>
    public ICollection<AccountStartingBalance> FindAllByAccount(Guid accountId) => _context.AccountStartingBalances
        .Where(accountStartingBalance => accountStartingBalance.AccountId == accountId)
        .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public ICollection<AccountStartingBalance> FindAllByAccountingPeriod(Guid accountingPeriodId) =>
        _context.AccountStartingBalances
        .Where(accountStartingBalance => accountStartingBalance.AccountingPeriodId == accountingPeriodId)
        .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public void Add(AccountStartingBalance accountStartingBalance)
    {
        var accountStartingBalanceData = PopulateFromAccountStartingBalance(accountStartingBalance, null);
        _context.Add(accountStartingBalanceData);
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        AccountStartingBalanceData accountStartingBalanceData = _context.AccountStartingBalances
            .Single(accountStartingBalance => accountStartingBalance.Id == id);
        _context.AccountStartingBalances.Remove(accountStartingBalanceData);
    }

    /// <summary>
    /// Converts the provided <see cref="AccountStartingBalanceData"/> object into 
    /// an <see cref="AccountStartingBalance"/> domain entity
    /// </summary>
    private AccountStartingBalance ConvertToEntity(AccountStartingBalanceData accountStartingBalanceData) =>
        _accountStartingBalanceFactory.Recreate(new AccountStartingDataRecreateRequest(accountStartingBalanceData.Id,
            accountStartingBalanceData.AccountId,
            accountStartingBalanceData.AccountingPeriodId,
            accountStartingBalanceData.StartingBalance));

    /// <summary>
    /// Converts the provided <see cref="AccountStartingBalance"/> entity into an <see cref="AccountStartingBalanceData"/>
    /// data model
    /// </summary>
    private static AccountStartingBalanceData PopulateFromAccountStartingBalance(
        AccountStartingBalance accountStartingBalance,
        AccountStartingBalanceData? existingAccountStartingBalanceData)
    {
        AccountStartingBalanceData newAccountStartingBalanceData = new AccountStartingBalanceData
        {
            Id = accountStartingBalance.Id,
            AccountId = accountStartingBalance.AccountId,
            AccountingPeriodId = accountStartingBalance.AccountingPeriodId,
            StartingBalance = accountStartingBalance.StartingBalance
        };
        existingAccountStartingBalanceData?.Replace(newAccountStartingBalanceData);
        return existingAccountStartingBalanceData ?? newAccountStartingBalanceData;
    }

    private sealed record AccountStartingDataRecreateRequest(Guid Id,
        Guid AccountId,
        Guid AccountingPeriodId,
        decimal StartingBalance) : IRecreateAccountStartingBalanceRequest;
}