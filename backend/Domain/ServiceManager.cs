using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

/// <summary>
/// Static class for managing all the DI services required for the Domain assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Domain DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<AccountingPeriodService>();
        _ = serviceCollection.AddScoped<AccountService>();
        _ = serviceCollection.AddScoped<AccountBalanceService>();
        _ = serviceCollection.AddScoped<FundService>();
        _ = serviceCollection.AddScoped<TransactionService>();
    }
}