using Data.EntityModels;
using Domain.Entities;
using Domain.Repositories;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Accounting Periods to be persisted to the database
/// </summary>
public class AccountingPeriodRepository : IAccountingPeriodRepository
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public AccountingPeriodRepository(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountingPeriod> FindAll() => _context.AccountingPeriods
        .AsEnumerable()
        .OrderBy(data => new DateTime(data.Year, data.Month, 1))
        .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public AccountingPeriod? FindOrNull(Guid id)
    {
        AccountingPeriodData? accountingPeriodData = _context.AccountingPeriods.FirstOrDefault(accountingPeriod => accountingPeriod.Id == id);
        return accountingPeriodData != null ? ConvertToEntity(accountingPeriodData) : null;
    }

    /// <inheritdoc/>
    public AccountingPeriod? FindOrNullByDate(DateOnly asOfDate)
    {
        AccountingPeriodData? accountingPeriodData = _context.AccountingPeriods
            .FirstOrDefault(accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);
        return accountingPeriodData != null ? ConvertToEntity(accountingPeriodData) : null;
    }

    /// <inheritdoc/>
    public ICollection<AccountingPeriod> FindOpenPeriods() =>
        _context.AccountingPeriods.Where(accountingPeriod => accountingPeriod.IsOpen).Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public AccountingPeriod FindEffectiveAccountingPeriodForBalances(DateOnly asOfDate)
    {
        AccountingPeriodData accountingPeriod = _context.AccountingPeriods.Single(
            accountingPeriod => accountingPeriod.Year == asOfDate.Year && accountingPeriod.Month == asOfDate.Month);
        if (!accountingPeriod.IsOpen)
        {
            return ConvertToEntity(accountingPeriod);
        }
        return ConvertToEntity(_context.AccountingPeriods.Where(accountingPeriod => accountingPeriod.IsOpen)
            .OrderBy(accountingPeriod => accountingPeriod.Year)
            .ThenBy(accountingPeriod => accountingPeriod.Month)
            .First());
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

    /// <summary>
    /// Converts the provided <see cref="AccountingPeriodData"/> object into an <see cref="AccountingPeriod"/> domain entity.
    /// </summary>
    /// <param name="accountingPeriodData">Accounting Period Data to be converted</param>
    /// <returns>The converted Accounting Period domain entity</returns>
    private AccountingPeriod ConvertToEntity(AccountingPeriodData accountingPeriodData) => new AccountingPeriod(
        new AccountingPeriodRecreateRequest
        {
            Id = accountingPeriodData.Id,
            Year = accountingPeriodData.Year,
            Month = accountingPeriodData.Month,
            IsOpen = accountingPeriodData.IsOpen,
        });

    /// <summary>
    /// Converts the provided <see cref="AccountingPeriod"/> entity into an <see cref="AccountingPeriodData"/> data object
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period entity to convert</param>
    /// <param name="existingAccountingPeriodData">Existing Accounting Period Data model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted Accounting Period Data</returns>
    private static AccountingPeriodData PopulateFromAccountingPeriod(AccountingPeriod accountingPeriod, AccountingPeriodData? existingAccountingPeriodData)
    {
        AccountingPeriodData newAccountingPeriodData = new AccountingPeriodData()
        {
            Id = accountingPeriod.Id,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
        };
        existingAccountingPeriodData?.Replace(newAccountingPeriodData);
        return existingAccountingPeriodData ?? newAccountingPeriodData;
    }

    /// <inheritdoc/>
    private sealed record AccountingPeriodRecreateRequest : IRecreateAccountingPeriodRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required int Year { get; init; }

        /// <inheritdoc/>
        public required int Month { get; init; }

        /// <inheritdoc/>
        public required bool IsOpen { get; init; }
    };
}