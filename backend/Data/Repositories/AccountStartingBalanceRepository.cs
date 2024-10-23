using Data.EntityModels;
using Data.Requests;
using Data.ValueObjectModels;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Account Starting Balances to be persisted to the database
/// </summary>
public class AccountStartingBalanceRepository : IAccountStartingBalanceRepository
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public AccountStartingBalanceRepository(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public AccountStartingBalance? FindOrNull(Guid accountId, Guid accountingPeriodId)
    {
        AccountStartingBalanceData? accountStartingBalanceData = _context.AccountStartingBalances
            .Include(accountStartingBalance => accountStartingBalance.StartingFundBalances)
            .SingleOrDefault(accountStartingBalance => accountStartingBalance.AccountId == accountId &&
                accountStartingBalance.AccountingPeriodId == accountingPeriodId);
        return accountStartingBalanceData != null ? ConvertToEntity(accountStartingBalanceData) : null;
    }

    /// <inheritdoc/>
    public void Add(AccountStartingBalance accountStartingBalance)
    {
        var accountStartingBalanceData = PopulateFromAccountStartingBalance(accountStartingBalance, null);
        _context.Add(accountStartingBalanceData);
    }

    /// <summary>
    /// Converts the provided <see cref="AccountStartingBalanceData"/> object into 
    /// an <see cref="AccountStartingBalance"/> domain entity
    /// </summary>
    /// <param name="accountStartingBalanceData">Account Starting Balance Data to be converted</param>
    /// <returns>The converted Account Starting Balance domain entity</returns>
    private static AccountStartingBalance ConvertToEntity(AccountStartingBalanceData accountStartingBalanceData) =>
        new AccountStartingBalance(new AccountStartingBalanceRecreateRequest
        {
            Id = accountStartingBalanceData.Id,
            AccountId = accountStartingBalanceData.AccountId,
            AccountingPeriodId = accountStartingBalanceData.AccountingPeriodId,
            StartingFundBalances = accountStartingBalanceData.StartingFundBalances
                .Select(startingBalance => new FundAmountRecreateRequest
                {
                    FundId = startingBalance.FundId,
                    Amount = startingBalance.Amount,
                }),
        });

    /// <summary>
    /// Converts the provided <see cref="AccountStartingBalance"/> entity into an <see cref="AccountStartingBalanceData"/>
    /// data object
    /// </summary>
    /// <param name="accountStartingBalance">Account Starting Balance entity to convert</param>
    /// <param name="existingAccountStartingBalanceData">Existing Account Starting Balance Data model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted Account Starting Balance Data</returns>
    private static AccountStartingBalanceData PopulateFromAccountStartingBalance(
        AccountStartingBalance accountStartingBalance,
        AccountStartingBalanceData? existingAccountStartingBalanceData)
    {
        AccountStartingBalanceData newAccountStartingBalanceData = new AccountStartingBalanceData
        {
            Id = accountStartingBalance.Id,
            AccountId = accountStartingBalance.AccountId,
            AccountingPeriodId = accountStartingBalance.AccountingPeriodId,
            StartingFundBalances = accountStartingBalance.StartingFundBalances
                .Select(startingBalance => new FundAmountData
                {
                    FundId = startingBalance.FundId,
                    Amount = startingBalance.Amount,
                }).ToList(),
        };
        existingAccountStartingBalanceData?.Replace(newAccountStartingBalanceData);
        return existingAccountStartingBalanceData ?? newAccountStartingBalanceData;
    }
}