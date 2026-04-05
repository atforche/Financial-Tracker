using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Budgets;
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
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<AccountingPeriodService>();
        _ = serviceCollection.AddScoped<AccountingPeriodBalanceService>();
        _ = serviceCollection.AddScoped<AccountService>();
        _ = serviceCollection.AddScoped<AccountBalanceService>();
        _ = serviceCollection.AddScoped<BudgetService>();
        _ = serviceCollection.AddScoped<BudgetBalanceService>();
        _ = serviceCollection.AddScoped<FundService>();
        _ = serviceCollection.AddScoped<FundBalanceService>();
        _ = serviceCollection.AddScoped<TransactionService>();
    }
}