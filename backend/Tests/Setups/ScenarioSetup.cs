using Data;
using Data.Repositories;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Services;
using Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Tests.Mocks;

namespace Tests.Setups;

/// <summary>
/// Abstract base class representing a Scenario Setup
/// </summary>
internal abstract class ScenarioSetup : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly bool shouldUseDatabase = Environment.GetEnvironmentVariable("USE_DATABASE") == "TRUE";

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    protected ScenarioSetup()
    {
        var serviceCollection = new ServiceCollection();

        if (shouldUseDatabase)
        {
            if (!Directory.Exists(TestDatabaseContext.DatabaseDirectory))
            {
                Directory.CreateDirectory(TestDatabaseContext.DatabaseDirectory);
            }
            serviceCollection.AddDbContext<DatabaseContext, TestDatabaseContext>();
            serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
            serviceCollection.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
            serviceCollection.AddScoped<IFundRepository, FundRepository>();
            serviceCollection.AddScoped<IBalanceEventRepository, BalanceEventRepository>();
            serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        }
        else
        {
            serviceCollection.AddScoped<IAccountRepository, MockAccountRepository>();
            serviceCollection.AddScoped<IAccountingPeriodRepository, MockAccountingPeriodRepository>();
            serviceCollection.AddScoped<IFundRepository, MockFundRepository>();
            serviceCollection.AddScoped<IBalanceEventRepository, MockBalanceEventRepository>();
            serviceCollection.AddScoped<ITransactionRepository, MockTransactionRepository>();
        }
        serviceCollection.AddScoped<TestUnitOfWork>();
        serviceCollection.AddScoped<AddAccountingPeriodAction>();
        serviceCollection.AddScoped<CloseAccountingPeriodAction>();
        serviceCollection.AddScoped<TransactionFactory>();
        serviceCollection.AddScoped<TransactionBalanceEventFactory>();
        serviceCollection.AddScoped<PostTransactionAction>();
        serviceCollection.AddScoped<AccountAddedBalanceEventFactory>();
        serviceCollection.AddScoped<ChangeInValueFactory>();
        serviceCollection.AddScoped<FundConversionFactory>();
        serviceCollection.AddScoped<AccountBalanceService>();
        serviceCollection.AddScoped<AccountFactory>();
        serviceCollection.AddScoped<AddFundAction>();
        _serviceProvider = serviceCollection.BuildServiceProvider();

        if (shouldUseDatabase)
        {
            GetService<DatabaseContext>().Database.EnsureCreated();
        }
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    public TService GetService<TService>() => _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (shouldUseDatabase)
        {
            GetService<DatabaseContext>().Database.EnsureDeleted();
        }
    }
}