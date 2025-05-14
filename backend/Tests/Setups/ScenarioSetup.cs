using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Tests.Mocks;

namespace Tests.Setups;

/// <summary>
/// Abstract base class representing a Scenario Setup
/// </summary>
internal abstract class ScenarioSetup
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    protected ScenarioSetup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IAccountRepository, MockAccountRepository>();
        serviceCollection.AddScoped<IAccountingPeriodRepository, MockAccountingPeriodRepository>();
        serviceCollection.AddScoped<IFundRepository, MockFundRepository>();
        serviceCollection.AddScoped<AddAccountingPeriodAction>();
        serviceCollection.AddScoped<CloseAccountingPeriodAction>();
        serviceCollection.AddScoped<AddTransactionAction>();
        serviceCollection.AddScoped<AddChangeInValueAction>();
        serviceCollection.AddScoped<AddFundConversionAction>();
        serviceCollection.AddScoped<AccountBalanceService>();
        serviceCollection.AddScoped<AddAccountAction>();
        serviceCollection.AddScoped<AddFundAction>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    public TService GetService<TService>() => _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();
}