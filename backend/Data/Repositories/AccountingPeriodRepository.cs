using Data.EntityModels;
using Domain.Entities;
using Domain.Events;
using Domain.Factories;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Accounting period repository that allows accounting periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly DatabaseContext _context;
    private readonly IAccountingPeriodFactory _accountingPeriodFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    /// <param name="accountingPeriodFactory">Factory used to construct Accounting Period instances</param>
    public AccountingPeriodRepository(DatabaseContext context, IAccountingPeriodFactory accountingPeriodFactory)
    {
        _context = context;
        _accountingPeriodFactory = accountingPeriodFactory;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => _context.AccountingPeriods
        .AsEnumerable()
        .OrderBy(data => new DateTime(data.Year, data.Month, 1))
        .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public AccountingPeriod Find(Guid id) => FindOrNull(id) ?? throw new KeyNotFoundException();

    /// <inheritdoc/>
    public AccountingPeriod? FindOrNull(Guid id)
    {
        AccountingPeriodData? accountingPeriodData = _context.AccountingPeriods.FirstOrDefault(accountingPeriod => accountingPeriod.Id == id);
        return accountingPeriodData != null ? ConvertToEntity(accountingPeriodData) : null;
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindOpenPeriod()
    {
        AccountingPeriodData? accountingPeriodData = _context.AccountingPeriods.SingleOrDefault(accountingPeriod => accountingPeriod.IsOpen);
        return accountingPeriodData != null ? ConvertToEntity(accountingPeriodData) : null;
    }

    /// <inheritdoc/>
    public void Add(AccountingPeriod accountingPeriod)
    {
        var accountingPeriodData = PopulateFromAccountingPeriod(accountingPeriod, null);
        _context.Add(accountingPeriodData);
    }

    /// <inheritdoc/>
    public void Update(AccountingPeriod accountingPeriod)
    {
        AccountingPeriodData accountingPeriodData = _context.AccountingPeriods.Single(accountingPeriodData => accountingPeriodData.Id == accountingPeriod.Id);
        PopulateFromAccountingPeriod(accountingPeriod, accountingPeriodData);
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        AccountingPeriodData accountingPeriodData = _context.AccountingPeriods.Single(accountingPeriodData => accountingPeriodData.Id == id);
        accountingPeriodData.RaiseEvent(new AccountingPeriodChangedEvent
        {
            Year = accountingPeriodData.Year,
            Month = accountingPeriodData.Month,
            Action = AccountingPeriodChangedAction.Deleted,
        });
        _context.AccountingPeriods.Remove(accountingPeriodData);
    }

    /// <summary>
    /// Converts the provided <see cref="AccountingPeriodData"/> object into an <see cref="AccountingPeriod"/> domain entity.
    /// </summary>
    private AccountingPeriod ConvertToEntity(AccountingPeriodData accountingPeriodData) => _accountingPeriodFactory.Recreate(
        new AccountingPeriodRecreateRequest(accountingPeriodData.Id, accountingPeriodData.Year, accountingPeriodData.Month, accountingPeriodData.IsOpen));

    /// <summary>
    /// Converts the provided <see cref="AccountingPeriod"/> entity into an <see cref="AccountingPeriodData"/> data object
    /// </summary>
    private static AccountingPeriodData PopulateFromAccountingPeriod(AccountingPeriod accountingPeriod, AccountingPeriodData? existingAccountPeriodData)
    {
        AccountingPeriodData newAccountingPeriodData = new AccountingPeriodData()
        {
            Id = accountingPeriod.Id,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
        };
        existingAccountPeriodData?.Replace(newAccountingPeriodData);

        AccountingPeriodData accountingPeriodData = existingAccountPeriodData ?? newAccountingPeriodData;
        foreach (IDomainEvent domainEvent in accountingPeriod.GetDomainEvents())
        {
            accountingPeriodData.RaiseEvent(domainEvent);
        }
        return accountingPeriodData;
    }

    private sealed record AccountingPeriodRecreateRequest(Guid Id, int Year, int Month, bool IsOpen) : IRecreateAccountingPeriodRequest;
}