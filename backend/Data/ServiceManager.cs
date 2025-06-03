using Data.Repositories;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.ChangeInValues;
using Domain.FundConversions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>
/// Static class for managing all the DI services required for the Data assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Data DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddDbContext<DatabaseContext>();
        _ = serviceCollection.AddScoped<UnitOfWork>();

        _ = serviceCollection.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        _ = serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
        _ = serviceCollection.AddScoped<IBalanceEventRepository, BalanceEventRepository>();
        _ = serviceCollection.AddScoped<IChangeInValueRepository, ChangeInValueRepository>();
        _ = serviceCollection.AddScoped<IFundConversionRepository, FundConversionRepository>();
        _ = serviceCollection.AddScoped<IFundRepository, FundRepository>();
        _ = serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
    }
}