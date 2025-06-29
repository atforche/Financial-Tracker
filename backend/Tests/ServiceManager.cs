using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.ChangeInValues;
using Domain.FundConversions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Builders;
using Tests.Mocks;

namespace Tests;

/// <summary>
/// Static class for managing all the DI services required for the Tests assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Test DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<TestUnitOfWork>();
        _ = serviceCollection.AddScoped<AccountingPeriodBuilder>();
        _ = serviceCollection.AddScoped<AccountBuilder>();
        _ = serviceCollection.AddScoped<FundBuilder>();

        Domain.ServiceManager.Register(serviceCollection);

        if (!DatabaseFixture.ShouldUseDatabase)
        {
            _ = serviceCollection.AddScoped<IAccountingPeriodRepository, MockAccountingPeriodRepository>();
            _ = serviceCollection.AddScoped<IAccountRepository, MockAccountRepository>();
            _ = serviceCollection.AddScoped<IBalanceEventRepository, MockBalanceEventRepository>();
            _ = serviceCollection.AddScoped<IChangeInValueRepository, MockChangeInValueRepository>();
            _ = serviceCollection.AddScoped<IFundConversionRepository, MockFundConversionRepository>();
            _ = serviceCollection.AddScoped<IFundRepository, MockFundRepository>();
            _ = serviceCollection.AddScoped<ITransactionRepository, MockTransactionRepository>();
        }
        else
        {
            Data.ServiceManager.Register(serviceCollection);
            serviceCollection.RemoveAll<DatabaseContext>();
            _ = serviceCollection.AddDbContext<DatabaseContext, TestDatabaseContext>();
        }
    }
}