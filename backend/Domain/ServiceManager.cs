using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Domain.Funds.Queries;
using Domain.Locations;
using Domain.Locations.Queries;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Domain.Users;
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
        _ = serviceCollection.AddScoped<AccountingPeriodQueryService>();
        _ = serviceCollection.AddScoped<AccountingPeriodRangeService>();

        _ = serviceCollection.AddScoped<AccountService>();
        _ = serviceCollection.AddScoped<AccountBalanceService>();
        _ = serviceCollection.AddScoped<PendingAccountBalanceService>();
        _ = serviceCollection.AddScoped<AccountBalanceEventQueryService>();
        _ = serviceCollection.AddScoped<AccountQueryService>();

        _ = serviceCollection.AddScoped<FundService>();
        _ = serviceCollection.AddScoped<FundBalanceService>();
        _ = serviceCollection.AddScoped<PendingFundBalanceService>();
        _ = serviceCollection.AddScoped<FundBalanceEventQueryService>();
        _ = serviceCollection.AddScoped<FundQueryService>();

        _ = serviceCollection.AddScoped<FundGoalService>();
        _ = serviceCollection.AddScoped<FundGoalTotalsHistoryService>();
        _ = serviceCollection.AddScoped<PendingFundGoalTotalsService>();
        _ = serviceCollection.AddScoped<FundGoalBalanceEventQueryService>();
        _ = serviceCollection.AddScoped<FundGoalQueryService>();

        _ = serviceCollection.AddScoped<SpendingTransactionService>();
        _ = serviceCollection.AddScoped<IncomeTransactionService>();
        _ = serviceCollection.AddScoped<AccountTransactionService>();
        _ = serviceCollection.AddScoped<FundTransactionService>();
        _ = serviceCollection.AddScoped<TransactionDispatcherService>();
        _ = serviceCollection.AddScoped<Transactions.Queries.TransactionQueryService>();
        _ = serviceCollection.AddScoped<LocationService>();
        _ = serviceCollection.AddScoped<LocationQueryService>();
        _ = serviceCollection.AddScoped<UserManagementService>();
    }
}
