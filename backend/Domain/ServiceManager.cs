using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Goals;
using Domain.Onboarding;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
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
        _ = serviceCollection.AddScoped<FundService>();
        _ = serviceCollection.AddScoped<FundBalanceService>();
        _ = serviceCollection.AddScoped<GoalService>();
        _ = serviceCollection.AddScoped<OnboardingService>();
        _ = serviceCollection.AddScoped<SpendingTransactionService>();
        _ = serviceCollection.AddScoped<IncomeTransactionService>();
        _ = serviceCollection.AddScoped<AccountTransactionService>();
        _ = serviceCollection.AddScoped<FundTransactionService>();
        _ = serviceCollection.AddScoped<TransactionDispatcherService>();
    }
}