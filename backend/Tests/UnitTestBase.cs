using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Tests.Mocks;

namespace Tests;

/// <summary>
/// Abstract base class that all unit tests inherit from
/// </summary>
public abstract class UnitTestBase
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    protected UnitTestBase()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IAccountRepository, MockAccountRepository>();
        serviceCollection.AddScoped<IAccountingPeriodRepository, MockAccountingPeriodRepository>();
        serviceCollection.AddScoped<IFundRepository, MockFundRepository>();
        serviceCollection.AddScoped<AddAccountingPeriodAction>();
        serviceCollection.AddScoped<AddTransactionAction>();
        serviceCollection.AddScoped<IAccountBalanceService, AccountBalanceService>();
        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IFundService, FundService>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    protected TService GetService<TService>() =>
        _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();
}